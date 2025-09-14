// The MIT License (MIT)
//
// Copyright (c) 2022-present Vincent Miller
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Linq;
using Infohazard.Core;
using Infohazard.StateSystem;
using UnityEngine;

namespace SBR {
    /// <summary>
    /// Motor class for a humanoid character. Performs movement and collision using a Rigidbody.
    /// </summary>
    /// <remarks>
    /// The Rigidbody should be set to NOT use gravity.
    /// It will not respond to forces; to apply force, set the CharacterMotor's velocity directly.
    /// </remarks>
    [RequireComponent(typeof(CapsuleCollider), typeof(Rigidbody))]
    public class CharacterMotor : Motor<CharacterChannels> {
        /// <summary>
        /// Capsule collider used by the character.
        /// </summary>
        public CapsuleCollider Capsule { get; private set; }

        /// <summary>
        /// Rigidbody collider used by the character.
        /// </summary>
        public Rigidbody Rigidbody { get; private set; }

        /// <summary>
        /// Animator used for root motion.
        /// </summary>
        public Animator Animator { get; private set; }

        /// <summary>
        /// Whether character is currently touching the ground.
        /// </summary>
        public bool IsGrounded { get; private set; }

        /// <summary>
        /// Whether character is currently sliding down a steep slope.
        /// </summary>
        public bool Sliding { get; private set; }

        /// <summary>
        /// Whether the character is currently jumping.
        /// </summary>
        public bool Jumping { get; private set; }

        /// <summary>
        /// Whether a jump is being charged.
        /// </summary>
        public bool JumpCharging { get; private set; }

        /// <summary>
        /// Timer until jump is fully charged.
        /// </summary>
        public ExpirationTimer JumpChargeTimer { get; private set; }

        /// <summary>
        /// Rotation rates of the character as local euler angles.
        /// </summary>
        public Vector3 RotationRates { get; private set; }

        public Vector3 Velocity {
            get => Rigidbody.GetLinearVelocity();
            set {
                if (!Rigidbody.isKinematic) Rigidbody.SetLinearVelocity(value);
            }
        }

        public Vector3 MovementVelocity => GetVelocityMovementComponent(Velocity);

        public bool IsMoving { get; private set; }

        public ExpirationTimer SnapPositionTimer { get; private set; }

        /// <summary>
        /// Called when the character jumps.
        /// </summary>
        public event Action Jumped;

        /// <summary>
        /// Called when the grounded state of the character changes.
        /// </summary>
        public event Action<bool> Grounded;

        /// <summary>
        /// Called when the character starts or stops moving.
        /// </summary>
        public event Action<bool> Moving;

        /// <summary>
        /// Current ground collider if grounded is true.
        /// </summary>
        public Collider Ground { get; private set; }

        /// <summary>
        /// Rigidbody of the ground collider if grounded is true.
        /// </summary>
        public Rigidbody GroundRigidbody { get; private set; }

        /// <summary>
        /// World-space gravity acceleration on this Character.
        /// </summary>
        public Vector3 Gravity => Physics.gravity * gravityScale;

        /// <summary>
        /// Current movement-based acceleration factoring in air control.
        /// </summary>
        public float ActualMovementAcceleration =>
            IsGrounded ? movementAcceleration : movementAcceleration * airAccelerationMultiplier;

        public float ActualMovementDeceleration =>
            IsGrounded ? movementDeceleration : movementDeceleration * airAccelerationMultiplier;

        public RaycastHit[] GroundHitBuffer { get; private set; }

        public RaycastHit GroundHit => _groundHit;
        private RaycastHit _groundHit;

        private Vector3 _groundNormal;
        private Vector3 _groundLastPos;
        private Vector3 _groundHitLocalPos;

        private Vector3 _rootMotionMovement;
        private Quaternion _rootMotionRotation = Quaternion.identity;

        private Vector3 _movementInput;

        private Quaternion _targetRotation = Quaternion.identity;
        private bool _hasInitializedTargetRotation = false;
        private bool _isRotating;
        private bool _hasInput;

        private AnimationCurve _snapPositionCurve;
        private Vector3 _snapStartPosition, _snapEndPosition;
        private Quaternion _snapStartRotation, _snapEndRotation;

        /// <summary>
        /// How the character rotates in relation to its movement.
        /// </summary>
        [Header("Rotation")]
        [Tooltip("How the character rotates in relation to its movement.")]
        public RotateMode rotateMode = RotateMode.Control;

        /// <summary>
        /// Whether to rotate faster when further from target rotation.
        /// </summary>
        [Tooltip("Whether to rotate faster when further from target rotation.")]
        [ConditionalDraw("rotateMode", RotateMode.None, false)]
        public bool useSmoothRotation = true;

        /// <summary>
        /// Speed at which the character rotates. Only used if rotate mode is set to Movement.
        /// </summary>
        [Tooltip("Speed at which the character rotates. Only used if rotate mode is set to Movement.")]
        [ConditionalDraw("rotateMode", RotateMode.None, false)]
        public float rotationSpeed = 15;

        /// <summary>
        /// Whether to rotate on the local X axis.
        /// </summary>
        [Tooltip("Whether to rotate on the local X axis.")]
        [ConditionalDraw("rotateMode", RotateMode.None, false)]
        public bool rotateX = false;

        /// <summary>
        /// Whether to rotate on the local Y axis.
        /// </summary>
        [Tooltip("Whether to rotate on the local Y axis.")]
        [ConditionalDraw("rotateMode", RotateMode.None, false)]
        public bool rotateY = true;

        /// <summary>
        /// Whether to rotate on the local Z axis.
        /// </summary>
        [Tooltip("Whether to rotate on the local Z axis.")]
        [ConditionalDraw("rotateMode", RotateMode.None, false)]
        public bool rotateZ = false;

        /// <summary>
        /// If angle between rotation and target is greater than this, will start rotating.
        /// </summary>
        [Tooltip("If angle between rotation and target is greater than this, will start rotating.")]
        [ConditionalDraw("rotateMode", RotateMode.None, false)]
        public float maxTargetAngle = 0;

        /// <summary>
        /// If angle between rotation and target is less than this, will stop rotating.
        /// </summary>
        [Tooltip("If angle between rotation and target is less than this, will stop rotating.")]
        [ConditionalDraw("rotateMode", RotateMode.None, false)]
        public float minTargetAngle = 0;

        /// <summary>
        /// The max movement speed of the character.
        /// </summary>
        [Tooltip("The max movement speed of the character.")]
        [Header("Walking")]
        public float movementSpeed = 5;

        /// <summary>
        /// The walking (ground) acceleration of the character.
        /// </summary>
        [Tooltip("The walking (ground) acceleration of the character.")]
        public float movementAcceleration = 25;

        /// <summary>
        /// The walking (ground) deceleration of the character.
        /// </summary>
        [Tooltip("The walking (ground) deceleration of the character.")]
        public float movementDeceleration = 20;

        /// <summary>
        /// Delay in seconds between receiving input and character actually moving.
        /// </summary>
        [Tooltip("Delay in seconds between receiving input and character actually moving.")] [SerializeField]
        private PassiveTimer _movementDelay;

        /// <summary>
        /// Multiplier to apply to movement speed while not grounded.
        /// </summary>
        [Tooltip("Multiplier to apply to movement speed while not grounded.")]
        public float airAccelerationMultiplier = 0.5f;

        /// <summary>
        /// How movement input affects the Motor's velocity.
        /// </summary>
        [Tooltip("How movement input affects the movement velocity.")]
        public InputAccelerationMode inputAccelerationMode = InputAccelerationMode.MoveVelocity;

        /// <summary>
        /// Damping factor for movement velocity.
        /// </summary>
        [Tooltip("Damping factor for movement velocity.")]
        public float movementVelocityDamping = 0;

        /// <summary>
        /// How to project the movement input vector.
        /// </summary>
        [Tooltip("How to project the movement input vector.")]
        public ProjectMovementMode projectMovement = ProjectMovementMode.GroundNormal;

        [Tooltip("Custom normal to project movement input onto.")]
        [ConditionalDraw(nameof(projectMovement), ProjectMovementMode.Custom)]
        public Vector3 customProjectMovementNormal = Vector3.up;

        /// <summary>
        /// The maximum slope, in degrees, that the character can climb.
        /// </summary>
        [Tooltip("The maximum slope, in degrees, that the character can climb.")]
        public float maxGroundAngle = 45;

        public bool slideOnSteepSlopes = true;

        /// <summary>
        /// How much the player will stick to the ground.
        /// </summary>
        [Range(0, 1)]
        public float groundStickiness = 0.5f;

        /// <summary>
        /// Whether the player will automatically stay on moving platforms.
        /// </summary>
        [Tooltip("Whether the character will automatically stay on moving platforms.")]
        public bool moveWithPlatforms = true;

        /// <summary>
        /// The speed at which the character jumps.
        /// </summary>
        [Header("Jumping / Falling")]
        [Tooltip("The local velocity at which the character jumps.")]
        public Vector3 jumpVelocity = Vector3.up * 4;

        /// <summary>
        /// How jumping works in regards to press vs hold.
        /// </summary>
        [Tooltip("How jumping works in regards to press vs hold.")]
        public JumpMode jumpMode = JumpMode.Press;

        /// <summary>
        /// How long the jump button must be held for max charge.
        /// </summary>
        [Tooltip("How long the jump button must be held for max charge.")]
        [ConditionalDraw(nameof(jumpMode), JumpMode.Charge)]
        public float jumpChargeTime = 0.5f;

        /// <summary>
        /// What fraction of the jump speed is used if the jump is not charged.
        /// </summary>
        [Tooltip("What fraction of the jump speed is used if the jump is not charged.")]
        [ConditionalDraw(nameof(jumpMode), JumpMode.Charge)]
        public float jumpChargeMinMultiplier = 0.2f;

        /// <summary>
        /// The value to multiply Physics.Gravity by.
        /// </summary>
        [Tooltip("The value to multiply Physics.Gravity by.")]
        public float gravityScale = 1;

        /// <summary>
        /// Distance for ground checks. Should be about 0.05 * capsule height.
        /// </summary>
        [Tooltip("Distance for ground checks. Should be about 0.05 * capsule height.")]
        public float groundDist = 0.1f;

        public int groundHitBufferSize = 8;

        [Range(0, 1)]
        public float groundCheckRadiusShell = 0.05f;

        /// <summary>
        /// Layers that are considered ground.
        /// </summary>
        [Tooltip("Layers that are considered ground.")]
        public LayerMask groundLayers = 1;

        /// <summary>
        /// Whether to apply root motion from animations in the XZ plane.
        /// </summary>
        [Header("Root Motion")]
        [Tooltip("Whether to apply root motion from animations in the XZ plane.")]
        public bool useRootMotionXZ = true;

        /// <summary>
        /// Whether to apply root motion from animations in the Y axis.
        /// </summary>
        [Tooltip("Whether to apply root motions from animations in the Y axis.")]
        public bool useRootMotionY = true;

        /// <summary>
        /// Whether to apply root rotation from animations.
        /// </summary>
        [Tooltip("Whether to apply root rotation from animations.")]
        public bool useRootMotionRotation = true;

        /// <summary>
        /// Scale to apply to root motion, so it has more or less effect.
        /// </summary>
        [Tooltip("Scale to apply to root motion, so it has more or less effect.")]
        public float rootMotionScale = 1.0f;

        /// <summary>
        /// StateManager to activate states on. If null, will only activate on self.
        /// </summary>
        [Header("State system")]
        [Tooltip("StateManager to activate states on. If null, will only activate on self.")]
        [NoOverrides]
        [SerializeField] private StateManager stateManager;

        /// <summary>
        /// State to activate when not grounded. Empty for no state.
        /// </summary>
        [Tooltip("State to activate when not grounded. Empty for no state.")]
        [NoOverrides]
        [SerializeField] private string notGroundedState = "Midair";

        /// <summary>
        /// State to activate when enableInput is false. Empty for no state.
        /// </summary>
        [Tooltip("State to activate when enableInput is false. Empty for no state.")]
        [NoOverrides]
        [SerializeField] private string noInputState = "NoInput";

        /// <summary>
        /// State to activate when moving. Empty for no state.
        /// </summary>
        [Tooltip("State to activate when moving. Empty for no state.")]
        [NoOverrides]
        [SerializeField] private string movingState = "Moving";

        public enum RotateMode {
            None, Movement, Control, Manual,
        }

        public enum JumpMode {
            Press, Charge
        }

        public enum ProjectMovementMode {
            None, LocalY, GroundNormal, Gravity, Custom,
        }

        public enum InputAccelerationMode {
            Instant, Force, MoveVelocity,
        }

        protected override void Awake() {
            base.Awake();

            Capsule = GetComponent<CapsuleCollider>();
            Rigidbody = GetComponent<Rigidbody>();
            Animator = GetComponent<Animator>();

            Rigidbody.useGravity = false;

            PhysicsMaterial frictionlessMaterial = new("Frictionless") {
                bounciness = 0,
                bounceCombine = PhysicsMaterialCombine.Minimum,
                staticFriction = 0,
                dynamicFriction = 0,
                frictionCombine = PhysicsMaterialCombine.Minimum,
            };
            Capsule.sharedMaterial = frictionlessMaterial;

            Time.fixedDeltaTime = 1.0f / 60.0f;
            GroundHitBuffer = new RaycastHit[groundHitBufferSize];

            if (jumpMode == JumpMode.Charge) {
                JumpChargeTimer = new ExpirationTimer(jumpChargeTime);
            }

            _movementDelay.Initialize();

            SetupStates();
        }

        #region State Functions
        private void SetStateOnManagerOrSelf(string state, bool active) {
            if (stateManager) {
                stateManager.SetStateActive(state, active);
            } else {
                SetStateActive(state, active);
            }
        }

        private void SetupStates() {
            if (!string.IsNullOrEmpty(movingState)) Moving += SetMovingState;
            if (!string.IsNullOrEmpty(notGroundedState)) Grounded += SetGroundedState;
            if (!string.IsNullOrEmpty(noInputState)) EnableInputChanged += SetEnableInputState;
        }

        private void SetMovingState(bool moving) => SetStateOnManagerOrSelf(movingState, moving);
        private void SetGroundedState(bool grounded) => SetStateOnManagerOrSelf(notGroundedState, !grounded);
        private void SetEnableInputState(bool value) => SetStateOnManagerOrSelf(noInputState, !value);
        #endregion

        private void Start() {
            UpdateGrounded();
        }

        private void Reset() {
            stateManager = GetComponent<StateManager>();
        }

        private void OnZeroHealth(bool isLoading) {
            stateManager.SetStateActive(movingState, false);
        }

        public void ApplyRootMotion(Vector3 deltaPosition, Quaternion deltaRotation) {
            _rootMotionMovement += deltaPosition;
            _rootMotionRotation = deltaRotation * _rootMotionRotation;
        }

        private void UpdateJumping() {
            if (Jumping && transform.InverseTransformDirection(Velocity).y <= 0) {
                Jumping = false;
                LastChannels.Jump = false;
            }
        }

        private void LateUpdate() {
            UpdateGrounded();
            UpdateJumping();

            _hasInput = EnableInput && Controllers.Any(c => c.enabled);
        }

        public void MoveTargetRotationTowards(Quaternion rotation) {
            if (!(rotateX && rotateY && rotateZ)) {
                Vector3 euler = (Quaternion.Inverse(transform.rotation) * rotation).eulerAngles;
                if (!rotateX) euler.x = 0;
                if (!rotateY) euler.y = 0;
                if (!rotateZ) euler.z = 0;
                _targetRotation = transform.rotation * Quaternion.Euler(euler);
            } else {
                _targetRotation = rotation;
            }

            _hasInitializedTargetRotation = true;
        }

        private void DoRotationOutput(CharacterChannels channels) {
            if (rotateMode == RotateMode.Movement) {
                if (channels.Movement.sqrMagnitude > 0) {
                    MoveTargetRotationTowards(Quaternion.LookRotation(channels.Movement, transform.up));
                }
            } else if (rotateMode == RotateMode.Control) {
                MoveTargetRotationTowards(channels.Rotation);
            }
        }

        private bool TryGetPlaneNormalForMovementProjection(out Vector3 normal) {
            switch (projectMovement) {
                case ProjectMovementMode.GroundNormal when IsGrounded:
                    normal = _groundNormal;
                    return true;
                case ProjectMovementMode.LocalY:
                case ProjectMovementMode.GroundNormal when !IsGrounded:
                    normal = Rigidbody.rotation * Vector3.up;
                    return true;
                case ProjectMovementMode.Gravity:
                    normal = -Physics.gravity;
                    return true;
                case ProjectMovementMode.Custom:
                    normal =  customProjectMovementNormal;
                    return true;
                default:
                    normal = Vector3.zero;
                    return false;
            }
        }

        private Vector3 AlignMovementInput(Vector3 input) {
            Vector3 result = input;

            if (TryGetPlaneNormalForMovementProjection(out Vector3 planeNormal)) {
                result = Vector3.ProjectOnPlane(input, planeNormal);
            }

            if (result.sqrMagnitude > 0.00001f)
                result = result.normalized * input.magnitude;

            return result;
        }

        private Vector3 GetVelocityMovementComponent(Vector3 vector) {
            Vector3 projectPlane = Physics.gravity;
            if (projectPlane.sqrMagnitude < 0.00001f) return vector;
            projectPlane = projectPlane.normalized;
            return Vector3.ProjectOnPlane(vector, projectPlane);
        }

        private Vector3 GetVelocityJumpComponent(Vector3 vector) {
            Vector3 projectPlane = Physics.gravity;
            if (projectPlane.sqrMagnitude < 0.00001f) return Vector3.zero;
            projectPlane = projectPlane.normalized;
            return Vector3.Project(vector, projectPlane);
        }

        public void SnapToLocationAndRotation(Vector3 position, Quaternion rotation, float time, AnimationCurve curve) {
            _snapPositionCurve = curve;

            _snapStartPosition = Rigidbody.position;
            _snapStartRotation = Rigidbody.rotation;
            _snapEndPosition = position;
            _snapEndRotation = rotation;

            if (SnapPositionTimer == null) SnapPositionTimer = new ExpirationTimer(time);
            else SnapPositionTimer.expiration = time;

            SnapPositionTimer.Set();
        }

        protected override void DoOutput(CharacterChannels channels) {
            DoRotationOutput(channels);
            _movementInput = channels.Movement;
            DoJumpOutput(channels);
        }

        private void DoJumpOutput(CharacterChannels channels) {
            if (IsGrounded && EnableInput && jumpVelocity.sqrMagnitude > 0) {
                if (channels.Jump) {
                    if (jumpMode == JumpMode.Press) {
                        DoJump(1);
                    } else if (jumpMode == JumpMode.Charge && !JumpCharging) {
                        JumpCharging = true;
                        JumpChargeTimer ??= new ExpirationTimer(jumpChargeTime);
                        JumpChargeTimer.Set();
                    }
                } else if (JumpCharging) {
                    float charge = 1.0f - JumpChargeTimer.remainingRatio;
                    DoJump(charge);
                }
            }

            if (!channels.Jump) {
                JumpCharging = false;
            }
        }

        public void DoJump(float charge) {
            Vector3 vel = transform.TransformDirection(jumpVelocity) * Mathf.Lerp(jumpChargeMinMultiplier, 1, charge);
            DoJump(vel);
        }

        public void DoJump(Vector3 velocity) {
            Jumped?.Invoke();
            Jumping = true;
            Velocity = velocity;
        }

        private void UpdateMovementVelocity(Vector3 input, float dt) {
            input = Vector3.ClampMagnitude(input, 1);
            input = AlignMovementInput(input);

            Vector3 desiredVelocity = input * movementSpeed;
            bool activeInput = input.sqrMagnitude > 0.0001f;
            float speedLimit = activeInput ? input.magnitude * movementSpeed : movementSpeed;

            Vector3 currentMovementVelocity = MovementVelocity;
            if (inputAccelerationMode == InputAccelerationMode.Instant) {
                currentMovementVelocity = desiredVelocity;
            } else {
                if (activeInput && currentMovementVelocity.sqrMagnitude < speedLimit * speedLimit + 0.0001f) {
                    // Player is holding input in a direction, and is not moving faster than the speed limit.
                    if (inputAccelerationMode == InputAccelerationMode.MoveVelocity) {
                        currentMovementVelocity =
                            Vector3.MoveTowards(currentMovementVelocity, desiredVelocity, ActualMovementAcceleration * dt);
                    } else if (inputAccelerationMode == InputAccelerationMode.Force) {
                        currentMovementVelocity += input.normalized * (ActualMovementAcceleration * dt);
                        currentMovementVelocity = Vector3.ClampMagnitude(currentMovementVelocity, speedLimit);
                    }
                } else {
                    // Player is not holding input, or is moving faster than the speed limit.
                    currentMovementVelocity =
                        Vector3.MoveTowards(currentMovementVelocity, desiredVelocity, ActualMovementDeceleration * dt);
                }
            }

            Vector3 projectedVel = GetVelocityMovementComponent(currentMovementVelocity);

            if (projectedVel.sqrMagnitude > 0.001f) {
                projectedVel = projectedVel.normalized * currentMovementVelocity.magnitude;
            }

            if (Sliding) {
                Vector3 n = Vector3.ProjectOnPlane(_groundNormal, Rigidbody.rotation * Vector3.up);
                n = -n.normalized;

                if (Vector3.Dot(projectedVel, n) > 0) {
                    Vector3 bad = Vector3.Project(projectedVel, n);
                    projectedVel -= bad;
                }
            }

            currentMovementVelocity = projectedVel;

            if (movementVelocityDamping > 0) {
                currentMovementVelocity *= Mathf.Clamp01(1 - movementVelocityDamping * dt);
            }

            float minMoveSpeed = movementSpeed * 0.1f;
            minMoveSpeed = minMoveSpeed * minMoveSpeed;

            bool newIsMoving = currentMovementVelocity.sqrMagnitude > minMoveSpeed;
            if (IsMoving != newIsMoving) {
                IsMoving = newIsMoving;
                Moving?.Invoke(newIsMoving);
            }

            Velocity = currentMovementVelocity + GetVelocityJumpComponent(Velocity);
        }

        public void UpdateGrounded() {
            bool isFixedUpdate = Time.inFixedTimeStep;
            var lastGround = Ground;
            if (Jumping) {
                IsGrounded = false;
                Ground = null;
                GroundRigidbody = null;
                if (lastGround) Grounded?.Invoke(false);
                return;
            }

            Matrix4x4 transformMatrix = isFixedUpdate
                ? Matrix4x4.TRS(Rigidbody.position, Rigidbody.rotation, Vector3.one)
                : transform.localToWorldMatrix;

            GameObjectUtility.GetCapsuleInfo(Capsule.radius, Capsule.height, Capsule.center, Capsule.direction,
                                             transformMatrix, out _, out Vector3 pnt2, out float radius, out _);

            Quaternion rotation = isFixedUpdate ? Rigidbody.rotation : transform.rotation;
            Vector3 up = rotation * Vector3.up;

            int hits = Physics.SphereCastNonAlloc(pnt2 + up * groundDist, radius, -up,
                GroundHitBuffer, groundDist * 2, groundLayers, QueryTriggerInteraction.Ignore);

            bool anyHit = false;
            IsGrounded = false;
            float maxPointDist = radius * (1 - groundCheckRadiusShell);
            maxPointDist *= maxPointDist;
            float maxGroundAngleDot = Mathf.Cos(maxGroundAngle);
            for (int i = 0; i < hits; i++) {
                RaycastHit hit = GroundHitBuffer[i];
                Vector3 proj = Vector3.ProjectOnPlane(hit.point - pnt2, up);

                if (proj.sqrMagnitude > maxPointDist) continue;

                bool angle = Vector3.Dot(hit.normal, up) >= maxGroundAngleDot;
                if (IsGrounded && !angle) continue;
                if ((IsGrounded || !angle) && anyHit && hit.distance >= _groundHit.distance) continue;

                _groundHit = hit;
                IsGrounded = angle;
                anyHit = true;
            }

            _groundHit.distance -= groundDist;
            Sliding = anyHit && !IsGrounded && slideOnSteepSlopes;

            if (anyHit) {
                _groundNormal = GroundHit.normal;
            }

            if (IsGrounded) {
                Ground = GroundHit.collider;
                GroundRigidbody = GroundHit.rigidbody;

                if (lastGround != Ground) {
                    if (GroundHit.rigidbody) {
                        _groundHitLocalPos = Quaternion.Inverse(GroundHit.rigidbody.rotation) *
                                            (GroundHit.point - GroundHit.rigidbody.position);
                    } else {
                        _groundHitLocalPos = Ground.transform.InverseTransformPoint(GroundHit.point);
                    }

                    _groundLastPos = GroundHit.point;
                }
            } else {
                Ground = null;
                GroundRigidbody = null;
            }

            if (lastGround != Ground) {
                Grounded?.Invoke(Ground);
            }
        }

        private void RotateTowardsTarget() {
            _targetRotation = _targetRotation.normalized;
            if (_isRotating) {
                Quaternion q = Rigidbody.rotation;
                if (rotationSpeed == 0) {
                    Rigidbody.MoveRotation(_targetRotation);
                } else if (useSmoothRotation) {
                    Rigidbody.MoveRotation(Quaternion.Slerp(Rigidbody.rotation, _targetRotation, Time.fixedDeltaTime * rotationSpeed));
                } else {
                    Rigidbody.MoveRotation(Quaternion.RotateTowards(Rigidbody.rotation, _targetRotation, Time.fixedDeltaTime * rotationSpeed));
                }
                RotationRates = MathUtility.NormalizeInnerAngles((Quaternion.Inverse(q) * Rigidbody.rotation).eulerAngles) / Time.fixedDeltaTime;

                if (minTargetAngle > 0 && Quaternion.Angle(Rigidbody.rotation, _targetRotation) <= minTargetAngle) {
                    _isRotating = false;
                }
            } else {
                if (Quaternion.Angle(Rigidbody.rotation, _targetRotation) > maxTargetAngle) {
                    _isRotating = true;
                }
                RotationRates = Vector3.zero;
            }
        }

        private void FixedUpdate() {
            float dt = Time.fixedDeltaTime;

            if (SnapPositionTimer?.expired == false) {
                Rigidbody.SetLinearVelocity(Vector3.zero);

                float t = _snapPositionCurve.Evaluate(1 - SnapPositionTimer.remainingRatio);
                Rigidbody.position = Vector3.Lerp(_snapStartPosition, _snapEndPosition, t);
                Rigidbody.rotation = Quaternion.Slerp(_snapStartRotation, _snapEndRotation, t);
                return;
            }

            bool hasMovementInput = _hasInput;
            bool inputIsNonzero = _movementInput != Vector3.zero;

            if (!(hasMovementInput && inputIsNonzero) &&
                _movementDelay.Interval > 0 &&
                MovementVelocity.sqrMagnitude < 0.01f) {
                _movementDelay.StartInterval();
            }

            if (!_movementDelay.IsIntervalEnded) {
                hasMovementInput = false;
            }

            UpdateMovementVelocity(hasMovementInput ? _movementInput : Vector3.zero, dt);

            if (!IsGrounded) {
                Rigidbody.AddLinearVelocity(Gravity * dt);
            }

            Vector3 theGroundIsMoving = GetPlatformMovement();
            Vector3 rootMovement = GetRootMovement();

            Rigidbody.MovePosition(Rigidbody.position + theGroundIsMoving + rootMovement);

            if (rotateMode != RotateMode.None && EnableInput && _hasInitializedTargetRotation) {
                RotateTowardsTarget();
            }
        }

        private Vector3 GetRootMovement() {
            Vector3 rootMovement = Vector3.zero;

            rootMovement = _rootMotionMovement * rootMotionScale;

            Vector3 up = Rigidbody.rotation * Vector3.up;

            if (!useRootMotionY) {
                rootMovement = Vector3.ProjectOnPlane(rootMovement, up);
            }

            if (!useRootMotionXZ) {
                rootMovement = Vector3.Project(rootMovement, up);
            }

            _rootMotionMovement = Vector3.zero;

            if (useRootMotionRotation) {
                Rigidbody.MoveRotation(_rootMotionRotation * Rigidbody.rotation);
            }

            _rootMotionRotation = Quaternion.identity;
            _rootMotionMovement = Vector3.zero;

            return rootMovement;
        }

        private Vector3 GetPlatformMovement() {
            Vector3 theGroundIsMoving = Vector3.zero;

            if (GroundRigidbody) {
                Vector3 curGroundHitPos = GroundRigidbody.position + GroundRigidbody.rotation * _groundHitLocalPos;

                if (moveWithPlatforms) theGroundIsMoving = curGroundHitPos - _groundLastPos;
                _groundLastPos = GroundHit.point;

                _groundHitLocalPos = Quaternion.Inverse(GroundRigidbody.rotation) *
                                     (GroundHit.point - GroundRigidbody.position);

                if (groundStickiness > 0 && !Jumping) {
                    float d = Mathf.Lerp(_groundHit.distance - groundDist, 0, groundStickiness);
                    Rigidbody.MovePosition(Rigidbody.position - (Rigidbody.rotation * Vector3.up) * (_groundHit.distance - d));
                    _groundHit.distance = d + groundDist;
                }
            }

            return theGroundIsMoving;
        }
    }
}
