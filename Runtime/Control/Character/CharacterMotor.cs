// The MIT License (MIT)
//
// Copyright (c) 2026-present Val Miller
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
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Serialization;

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

        [SerializeField]
        [FormerlySerializedAs("rotateMode")]
        [Header("Rotation")]
        [Tooltip("How the character rotates in relation to its movement.")]
        private CharacterRotateMode _rotateMode = CharacterRotateMode.Control;

        [SerializeField]
        [FormerlySerializedAs("useSmoothRotation")]
        [Tooltip("Whether to rotate faster when further from target rotation.")]
        [ConditionalDraw(nameof(_rotateMode), CharacterRotateMode.None, false)]
        private bool _useSmoothRotation = true;

        [SerializeField]
        [FormerlySerializedAs("rotationSpeed")]
        [Tooltip("Speed at which the character rotates. Only used if rotate mode is set to Movement.")]
        [ConditionalDraw(nameof(_rotateMode), CharacterRotateMode.None, false)]
        private float _rotationSpeed = 15;

        [SerializeField]
        [FormerlySerializedAs("rotateX")]
        [Tooltip("Whether to rotate on the local X axis.")]
        [ConditionalDraw(nameof(_rotateMode), CharacterRotateMode.None, false)]
        private bool _rotateX = false;

        [SerializeField]
        [FormerlySerializedAs("rotateY")]
        [Tooltip("Whether to rotate on the local Y axis.")]
        [ConditionalDraw(nameof(_rotateMode), CharacterRotateMode.None, false)]
        private bool _rotateY = true;

        [SerializeField]
        [FormerlySerializedAs("rotateZ")]
        [Tooltip("Whether to rotate on the local Z axis.")]
        [ConditionalDraw(nameof(_rotateMode), CharacterRotateMode.None, false)]
        private bool _rotateZ = false;

        [SerializeField]
        [Tooltip("When using rotateY and not rotateX, this is the minimum pitch offset for the character to match Y.")]
        private float _rotateYPitchMin = -80;

        [SerializeField]
        [Tooltip("When using rotateY and not rotateX, this is the maximum pitch offset for the character to match Y.")]
        private float _rotateYPitchMax = 80;

        [SerializeField]
        [FormerlySerializedAs("maxTargetAngle")]
        [Tooltip("If angle between rotation and target is greater than this, will start rotating.")]
        [ConditionalDraw(nameof(_rotateMode), CharacterRotateMode.None, false)]
        private float _maxTargetAngle = 0;

        [SerializeField]
        [FormerlySerializedAs("minTargetAngle")]
        [Tooltip("If angle between rotation and target is less than this, will stop rotating.")]
        [ConditionalDraw(nameof(_rotateMode), CharacterRotateMode.None, false)]
        private float _minTargetAngle = 0;

        [Header("Walking")]
        [SerializeField]
        [FormerlySerializedAs("movementSpeed")]
        [Tooltip("The max movement speed of the character.")]
        private float _movementSpeed = 5;

        [SerializeField]
        [FormerlySerializedAs("movementAcceleration")]
        [Tooltip("The walking (ground) acceleration of the character.")]
        private float _movementAcceleration = 25;

        [SerializeField]
        [FormerlySerializedAs("movementDeceleration")]
        [Tooltip("The walking (ground) deceleration of the character.")]
        private float _movementDeceleration = 20;

        [SerializeField]
        [Tooltip("Delay in seconds between receiving input and character actually moving.")]
        private PassiveTimer _movementDelay;

        [SerializeField]
        [FormerlySerializedAs("airAccelerationMultiplier")]
        [Tooltip("Multiplier to apply to movement speed while not grounded.")]
        private float _airAccelerationMultiplier = 0.5f;

        [SerializeField]
        [FormerlySerializedAs("inputAccelerationMode")]
        [Tooltip("How movement input affects the movement velocity.")]
        private CharacterInputAccelerationMode _inputAccelerationMode = CharacterInputAccelerationMode.MoveVelocity;

        [SerializeField]
        [FormerlySerializedAs("movementVelocityDamping")]
        [Tooltip("Damping factor for movement velocity.")]
        private float _movementVelocityDamping = 0;

        [SerializeField]
        [FormerlySerializedAs("_projectMovement")]
        [FormerlySerializedAs("projectMovement")]
        [Tooltip("How to project the movement input vector.")]
        private CharacterMovementProjectionMode _movementProjection = CharacterMovementProjectionMode.GroundNormal;

        [SerializeField]
        [FormerlySerializedAs("customProjectMovementNormal")]
        [Tooltip("Custom normal to project movement input onto.")]
        [ConditionalDraw(nameof(_movementProjection), CharacterMovementProjectionMode.Custom)]
        private Vector3 _customProjectMovementNormal = Vector3.up;

        [SerializeField]
        [FormerlySerializedAs("maxGroundAngle")]
        [Tooltip("The maximum slope, in degrees, that the character can climb.")]
        private float _maxGroundAngle = 45;

        [SerializeField]
        [FormerlySerializedAs("slideOnSteepSlopes")]
        [Tooltip("Whether the character will slide downwards when on steep slopes.")]
        private bool _slideOnSteepSlopes = true;

        [SerializeField]
        [FormerlySerializedAs("groundStickiness")]
        [Range(0, 1)]
        private float _groundStickiness = 0.5f;

        [SerializeField]
        [FormerlySerializedAs("moveWithPlatforms")]
        [Tooltip("Whether the character will automatically stay on moving platforms.")]
        private bool _moveWithPlatforms = true;

        [SerializeField]
        [FormerlySerializedAs("jumpVelocity")]
        [Header("Jumping / Falling")]
        [Tooltip("The local velocity at which the character jumps.")]
        private Vector3 _jumpVelocity = Vector3.up * 4;

        [SerializeField]
        [FormerlySerializedAs("jumpMode")]
        [Tooltip("How jumping works in regards to press vs hold.")]
        private CharacterJumpMode _jumpMode = CharacterJumpMode.Press;

        [SerializeField]
        [FormerlySerializedAs("jumpChargeTime")]
        [Tooltip("How long the jump button must be held for max charge.")]
        [ConditionalDraw(nameof(_jumpMode), CharacterJumpMode.Charge)]
        private float _jumpChargeTime = 0.5f;

        [SerializeField]
        [FormerlySerializedAs("jumpChargeMinMultiplier")]
        [Tooltip("What fraction of the jump speed is used if the jump is not charged.")]
        [ConditionalDraw(nameof(_jumpMode), CharacterJumpMode.Charge)]
        private float _jumpChargeMinMultiplier = 0.2f;

        [SerializeField]
        [FormerlySerializedAs("gravityScale")]
        [Tooltip("The value to multiply Physics.Gravity by.")]
        private float _gravityScale = 1;

        [SerializeField]
        [FormerlySerializedAs("groundDist")]
        [Tooltip("Distance for ground checks. Should be about 0.05 * capsule height.")]
        private float _groundDist = 0.1f;

        [SerializeField]
        [FormerlySerializedAs("groundHitBufferSize")]
        [Tooltip("How many ground hits to query for.")]
        private int _groundHitBufferSize = 8;

        [SerializeField]
        [FormerlySerializedAs("groundCheckRadiusShell")]
        [Range(0, 1)]
        [Tooltip("How much of the capsule radius to use for ground checks.")]
        private float _groundCheckRadiusShell = 0.05f;

        [SerializeField]
        [FormerlySerializedAs("groundLayers")]
        [Tooltip("Layers that are considered ground.")]
        private LayerMask _groundLayers = 1;

        [SerializeField]
        [FormerlySerializedAs("useRootMotionXZ")]
        [Header("Root Motion")]
        [Tooltip("Whether to apply root motion from animations in the XZ plane.")]
        private bool _useRootMotionXZ = true;

        [SerializeField]
        [FormerlySerializedAs("useRootMotionY")]
        [Tooltip("Whether to apply root motions from animations in the Y axis.")]
        private bool _useRootMotionY = true;

        [SerializeField]
        [FormerlySerializedAs("useRootMotionRotation")]
        [Tooltip("Whether to apply root rotation from animations.")]
        private bool _useRootMotionRotation = true;

        [SerializeField]
        [FormerlySerializedAs("rootMotionScale")]
        [Tooltip("Scale to apply to root motion, so it has more or less effect.")]
        private float _rootMotionScale = 1.0f;

        [SerializeField]
        [FormerlySerializedAs("stateManager")]
        [Header("State system")]
        [Tooltip("StateManager to activate states on. If null, will only activate on self.")]
        [NoOverrides]
        private StateManager _stateManager;

        [SerializeField]
        [FormerlySerializedAs("notGroundedState")]
        [Tooltip("State to activate when not grounded. Empty for no state.")]
        [NoOverrides]
        private string _notGroundedState = "Midair";

        [SerializeField]
        [FormerlySerializedAs("noInputState")]
        [Tooltip("State to activate when enableInput is false. Empty for no state.")]
        [NoOverrides]
        private string _noInputState = "NoInput";

        [SerializeField]
        [FormerlySerializedAs("movingState")]
        [Tooltip("State to activate when moving. Empty for no state.")]
        [NoOverrides]
        private string _movingState = "Moving";

        /// <summary>
        /// How the character rotates in relation to its movement.
        /// </summary>
        public CharacterRotateMode RotateMode {
            get => _rotateMode;
            set => _rotateMode = value;
        }

        /// <summary>
        /// Whether to rotate faster when further from the target rotation.
        /// </summary>
        public bool UseSmoothRotation {
            get => _useSmoothRotation;
            set => _useSmoothRotation = value;
        }

        /// <summary>
        /// Speed at which the character rotates toward its target rotation. A value of 0 snaps to the target immediately.
        /// </summary>
        public float RotationSpeed {
            get => _rotationSpeed;
            set => _rotationSpeed = value;
        }

        /// <summary>
        /// Whether the character rotates on its local X axis.
        /// </summary>
        public bool RotateX {
            get => _rotateX;
            set => _rotateX = value;
        }

        /// <summary>
        /// Whether the character rotates on its local Y axis.
        /// </summary>
        public bool RotateY {
            get => _rotateY;
            set => _rotateY = value;
        }

        /// <summary>
        /// Whether the character rotates on its local Z axis.
        /// </summary>
        public bool RotateZ {
            get => _rotateZ;
            set => _rotateZ = value;
        }

        /// <summary>
        /// Once rotating, the character stops when the angle to its target rotation is at or below this value (degrees).
        /// Ignored when 0 or less.
        /// </summary>
        public float MinTargetAngle {
            get => _minTargetAngle;
            set => _minTargetAngle = value;
        }

        /// <summary>
        /// The max movement speed of the character.
        /// </summary>
        public float MovementSpeed {
            get => _movementSpeed;
            set => _movementSpeed = value;
        }

        /// <summary>
        /// The walking (ground) acceleration of the character.
        /// </summary>
        public float MovementAcceleration {
            get => _movementAcceleration;
            set => _movementAcceleration = value;
        }

        /// <summary>
        /// The walking (ground) deceleration of the character.
        /// </summary>
        public float MovementDeceleration {
            get => _movementDeceleration;
            set => _movementDeceleration = value;
        }

        /// <summary>
        /// Timer for the delay between receiving movement input and the character actually moving.
        /// </summary>
        public ref PassiveTimer MovementDelay => ref _movementDelay;

        /// <summary>
        /// Multiplier applied to movement acceleration and deceleration while not grounded.
        /// </summary>
        public float AirAccelerationMultiplier {
            get => _airAccelerationMultiplier;
            set => _airAccelerationMultiplier = value;
        }

        /// <summary>
        /// How movement input affects the movement velocity.
        /// </summary>
        public CharacterInputAccelerationMode InputAccelerationMode {
            get => _inputAccelerationMode;
            set => _inputAccelerationMode = value;
        }

        /// <summary>
        /// Damping factor applied to movement velocity. A value of 0 disables damping.
        /// </summary>
        public float MovementVelocityDamping {
            get => _movementVelocityDamping;
            set => _movementVelocityDamping = value;
        }

        /// <summary>
        /// How the movement input vector is projected.
        /// </summary>
        public CharacterMovementProjectionMode MovementProjection {
            get => _movementProjection;
            set => _movementProjection = value;
        }

        /// <summary>
        /// Normal that movement input is projected onto when <see cref="MovementProjection"/> is Custom.
        /// </summary>
        public Vector3 CustomProjectMovementNormal {
            get => _customProjectMovementNormal;
            set => _customProjectMovementNormal = value;
        }

        /// <summary>
        /// The maximum slope, in degrees, that the character can climb.
        /// </summary>
        public float MaxGroundAngle {
            get => _maxGroundAngle;
            set => _maxGroundAngle = value;
        }

        /// <summary>
        /// Whether the character slides downwards when on steep slopes.
        /// </summary>
        public bool SlideOnSteepSlopes {
            get => _slideOnSteepSlopes;
            set => _slideOnSteepSlopes = value;
        }

        /// <summary>
        /// How strongly the character is held to the ground, from 0 (none) to 1 (full).
        /// </summary>
        public float GroundStickiness {
            get => _groundStickiness;
            set => _groundStickiness = value;
        }

        /// <summary>
        /// Whether the character automatically stays on moving platforms.
        /// </summary>
        public bool MoveWithPlatforms {
            get => _moveWithPlatforms;
            set => _moveWithPlatforms = value;
        }

        /// <summary>
        /// The local velocity at which the character jumps.
        /// </summary>
        public Vector3 JumpVelocity {
            get => _jumpVelocity;
            set => _jumpVelocity = value;
        }

        /// <summary>
        /// How jumping works in regards to press vs hold.
        /// </summary>
        public CharacterJumpMode JumpMode {
            get => _jumpMode;
            set => _jumpMode = value;
        }

        /// <summary>
        /// How long the jump button must be held for max charge. Only used when <see cref="JumpMode"/> is Charge.
        /// </summary>
        public float JumpChargeTime {
            get => _jumpChargeTime;
            set => _jumpChargeTime = value;
        }

        /// <summary>
        /// The multiplier applied to Physics.Gravity for this character.
        /// </summary>
        public float GravityScale {
            get => _gravityScale;
            set => _gravityScale = value;
        }

        /// <summary>
        /// Distance for ground checks.
        /// </summary>
        public float GroundDist {
            get => _groundDist;
            set => _groundDist = value;
        }

        /// <summary>
        /// Fraction of the capsule radius excluded from ground checks.
        /// </summary>
        public float GroundCheckRadiusShell {
            get => _groundCheckRadiusShell;
            set => _groundCheckRadiusShell = value;
        }

        /// <summary>
        /// Layers that are considered ground.
        /// </summary>
        public LayerMask GroundLayers {
            get => _groundLayers;
            set => _groundLayers = value;
        }

        /// <summary>
        /// Whether to apply root motion from animations in the XZ plane.
        /// </summary>
        public bool UseRootMotionXZ {
            get => _useRootMotionXZ;
            set => _useRootMotionXZ = value;
        }

        /// <summary>
        /// Whether to apply root motion from animations on the Y axis.
        /// </summary>
        public bool UseRootMotionY {
            get => _useRootMotionY;
            set => _useRootMotionY = value;
        }

        /// <summary>
        /// Whether to apply root rotation from animations.
        /// </summary>
        public bool UseRootMotionRotation {
            get => _useRootMotionRotation;
            set => _useRootMotionRotation = value;
        }

        /// <summary>
        /// Scale applied to root motion, so it has more or less effect.
        /// </summary>
        public float RootMotionScale {
            get => _rootMotionScale;
            set => _rootMotionScale = value;
        }

        /// <summary>
        /// StateManager that states are activated on. Null means states are activated on this character.
        /// </summary>
        public StateManager StateManager => _stateManager;

        /// <summary>
        /// State activated while not grounded. Empty for no state.
        /// </summary>
        public string NotGroundedState => _notGroundedState;

        /// <summary>
        /// State activated while input is disabled. Empty for no state.
        /// </summary>
        public string NoInputState => _noInputState;

        /// <summary>
        /// State activated while moving. Empty for no state.
        /// </summary>
        public string MovingState => _movingState;

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

        /// <summary>
        /// The character's current linear velocity, applied through its rigidbody.
        /// </summary>
        public Vector3 Velocity {
            get => Rigidbody.GetLinearVelocity();
            set {
                if (!Rigidbody.isKinematic) Rigidbody.SetLinearVelocity(value);
            }
        }

        /// <summary>
        /// The current component of the character's velocity that is due to movement (as opposed to jumping/falling)
        /// </summary>
        public Vector3 MovementVelocity => GetVelocityMovementComponent(Velocity);

        /// <summary>
        /// Whether the character is currently moving.
        /// </summary>
        public bool IsMoving { get; private set; }

        /// <summary>
        /// Timer for snapping the character's position to a target point.
        /// </summary>
        public ExpirationTimer SnapPositionTimer { get; private set; }

        /// <summary>
        /// Buffer of ground hits for hit testing.
        /// </summary>
        public RaycastHit[] GroundHitBuffer { get; private set; }

        /// <summary>
        /// Ground hit result from the last frame.
        /// </summary>
        public RaycastHit GroundHit => _groundHit;

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
        public Vector3 Gravity => Physics.gravity * _gravityScale;

        /// <summary>
        /// Current movement-based acceleration factoring in air control.
        /// </summary>
        public float ActualMovementAcceleration =>
            IsGrounded ? _movementAcceleration : _movementAcceleration * _airAccelerationMultiplier;

        /// <summary>
        /// Current movement-based deceleration factoring in air control.
        /// </summary>
        public float ActualMovementDeceleration =>
            IsGrounded ? _movementDeceleration : _movementDeceleration * _airAccelerationMultiplier;

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
            GroundHitBuffer = new RaycastHit[_groundHitBufferSize];

            if (_jumpMode == CharacterJumpMode.Charge) {
                JumpChargeTimer = new ExpirationTimer(_jumpChargeTime);
            }

            _movementDelay.Initialize();

            SetupStates();
        }

        #region State Functions
        private void SetStateOnManagerOrSelf(string state, bool active) {
            if (_stateManager) {
                _stateManager.SetStateActive(state, active);
            } else {
                SetStateActive(state, active);
            }
        }

        private void SetupStates() {
            if (!string.IsNullOrEmpty(_movingState)) Moving += SetMovingState;
            if (!string.IsNullOrEmpty(_notGroundedState)) Grounded += SetGroundedState;
            if (!string.IsNullOrEmpty(_noInputState)) EnableInputChanged += SetEnableInputState;
        }

        private void SetMovingState(bool moving) => SetStateOnManagerOrSelf(_movingState, moving);
        private void SetGroundedState(bool grounded) => SetStateOnManagerOrSelf(_notGroundedState, !grounded);
        private void SetEnableInputState(bool value) => SetStateOnManagerOrSelf(_noInputState, !value);
        #endregion

        private void Start() {
            UpdateGrounded();
        }

        private void Reset() {
            _stateManager = GetComponent<StateManager>();
        }

        [UsedImplicitly]
        private void OnZeroHealth(bool isLoading) {
            _stateManager.SetStateActive(_movingState, false);
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

            _hasInput = EnableInput && Controllers.Any(c => c.Enabled);
        }

        public void MoveTargetRotationTowards(Quaternion rotation) {
            if (!(_rotateX && _rotateY && _rotateZ)) {
                Vector3 euler = (Quaternion.Inverse(transform.rotation) * rotation).eulerAngles;
                float eulerX = MathUtility.NormalizeInnerAngle(euler.x);
                if (!_rotateX) euler.x = 0;
                if (!_rotateY) euler.y = 0;
                if (!_rotateZ) euler.z = 0;

                if (_rotateY && !_rotateX && (eulerX < _rotateYPitchMin || eulerX > _rotateYPitchMax)) {
                    euler.y = 0;
                }

                _targetRotation = transform.rotation * Quaternion.Euler(euler);
            } else {
                _targetRotation = rotation;
            }

            _hasInitializedTargetRotation = true;
        }

        private void DoRotationOutput(CharacterChannels channels) {
            if (_rotateMode == CharacterRotateMode.Movement) {
                if (channels.Movement.sqrMagnitude > 0) {
                    MoveTargetRotationTowards(Quaternion.LookRotation(channels.Movement, transform.up));
                }
            } else if (_rotateMode == CharacterRotateMode.Control) {
                MoveTargetRotationTowards(channels.Rotation);
            }
        }

        private bool TryGetPlaneNormalForMovementProjection(out Vector3 normal) {
            switch (_movementProjection) {
                case CharacterMovementProjectionMode.GroundNormal when IsGrounded:
                    normal = _groundNormal;
                    return true;
                case CharacterMovementProjectionMode.LocalY:
                case CharacterMovementProjectionMode.GroundNormal when !IsGrounded:
                    normal = Rigidbody.rotation * Vector3.up;
                    return true;
                case CharacterMovementProjectionMode.Gravity:
                    normal = -Physics.gravity;
                    return true;
                case CharacterMovementProjectionMode.Custom:
                    normal =  _customProjectMovementNormal;
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
            else SnapPositionTimer.Expiration = time;

            SnapPositionTimer.Set();
        }

        public void CancelSnapPosition() {
            SnapPositionTimer?.Clear();
        }

        protected override void DoOutput(CharacterChannels channels) {
            DoRotationOutput(channels);
            _movementInput = channels.Movement;
            DoJumpOutput(channels);
        }

        private void DoJumpOutput(CharacterChannels channels) {
            if (IsGrounded && EnableInput && _jumpVelocity.sqrMagnitude > 0) {
                if (channels.Jump) {
                    if (_jumpMode == CharacterJumpMode.Press) {
                        DoJump(1);
                    } else if (_jumpMode == CharacterJumpMode.Charge && !JumpCharging) {
                        JumpCharging = true;
                        JumpChargeTimer ??= new ExpirationTimer(_jumpChargeTime);
                        JumpChargeTimer.Set();
                    }
                } else if (JumpCharging) {
                    float charge = 1.0f - JumpChargeTimer.RemainingRatio;
                    DoJump(charge);
                }
            }

            if (!channels.Jump) {
                JumpCharging = false;
            }
        }

        public void DoJump(float charge) {
            Vector3 vel = transform.TransformDirection(_jumpVelocity) * Mathf.Lerp(_jumpChargeMinMultiplier, 1, charge);
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

            Vector3 desiredVelocity = input * _movementSpeed;
            bool activeInput = input.sqrMagnitude > 0.0001f;
            float speedLimit = activeInput ? input.magnitude * _movementSpeed : _movementSpeed;

            Vector3 currentMovementVelocity = MovementVelocity;
            if (_inputAccelerationMode == CharacterInputAccelerationMode.Instant) {
                currentMovementVelocity = desiredVelocity;
            } else {
                if (activeInput && currentMovementVelocity.sqrMagnitude < speedLimit * speedLimit + 0.0001f) {
                    // Player is holding input in a direction, and is not moving faster than the speed limit.
                    if (_inputAccelerationMode == CharacterInputAccelerationMode.MoveVelocity) {
                        currentMovementVelocity =
                            Vector3.MoveTowards(currentMovementVelocity, desiredVelocity, ActualMovementAcceleration * dt);
                    } else if (_inputAccelerationMode == CharacterInputAccelerationMode.Force) {
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

            if (_movementVelocityDamping > 0) {
                currentMovementVelocity *= Mathf.Clamp01(1 - _movementVelocityDamping * dt);
            }

            float minMoveSpeed = _movementSpeed * 0.1f;
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
            Collider lastGround = Ground;
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

            int hits = Physics.SphereCastNonAlloc(pnt2 + up * _groundDist, radius, -up,
                GroundHitBuffer, _groundDist * 2, _groundLayers, QueryTriggerInteraction.Ignore);

            bool anyHit = false;
            IsGrounded = false;
            float maxPointDist = radius * (1 - _groundCheckRadiusShell);
            maxPointDist *= maxPointDist;
            float maxGroundAngleDot = Mathf.Cos(_maxGroundAngle * Mathf.Deg2Rad);
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

            _groundHit.distance -= _groundDist;
            Sliding = anyHit && !IsGrounded && _slideOnSteepSlopes;

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
                if (_rotationSpeed == 0) {
                    Rigidbody.MoveRotation(_targetRotation);
                } else if (_useSmoothRotation) {
                    Rigidbody.MoveRotation(Quaternion.Slerp(Rigidbody.rotation, _targetRotation, Time.fixedDeltaTime * _rotationSpeed));
                } else {
                    Rigidbody.MoveRotation(Quaternion.RotateTowards(Rigidbody.rotation, _targetRotation, Time.fixedDeltaTime * _rotationSpeed));
                }
                RotationRates = MathUtility.NormalizeInnerAngles((Quaternion.Inverse(q) * Rigidbody.rotation).eulerAngles) / Time.fixedDeltaTime;

                if (_minTargetAngle > 0 && Quaternion.Angle(Rigidbody.rotation, _targetRotation) <= _minTargetAngle) {
                    _isRotating = false;
                }
            } else {
                if (Quaternion.Angle(Rigidbody.rotation, _targetRotation) > _maxTargetAngle) {
                    _isRotating = true;
                }
                RotationRates = Vector3.zero;
            }
        }

        private void FixedUpdate() {
            float dt = Time.fixedDeltaTime;

            if (SnapPositionTimer?.Expired == false) {
                Rigidbody.SetLinearVelocity(Vector3.zero);

                float t = _snapPositionCurve.Evaluate(1 - SnapPositionTimer.RemainingRatio);
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

            if (_rotateMode != CharacterRotateMode.None && EnableInput && _hasInitializedTargetRotation) {
                RotateTowardsTarget();
            }
        }

        private Vector3 GetRootMovement() {
            Vector3 rootMovement = _rootMotionMovement * _rootMotionScale;

            Vector3 up = Rigidbody.rotation * Vector3.up;

            if (!_useRootMotionY) {
                rootMovement = Vector3.ProjectOnPlane(rootMovement, up);
            }

            if (!_useRootMotionXZ) {
                rootMovement = Vector3.Project(rootMovement, up);
            }

            _rootMotionMovement = Vector3.zero;

            if (_useRootMotionRotation) {
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

                if (_moveWithPlatforms) theGroundIsMoving = curGroundHitPos - _groundLastPos;
                _groundLastPos = GroundHit.point;

                _groundHitLocalPos = Quaternion.Inverse(GroundRigidbody.rotation) *
                                     (GroundHit.point - GroundRigidbody.position);

                if (_groundStickiness > 0 && !Jumping) {
                    float d = Mathf.Lerp(_groundHit.distance - _groundDist, 0, _groundStickiness);
                    Rigidbody.MovePosition(Rigidbody.position - (Rigidbody.rotation * Vector3.up) * (_groundHit.distance - d));
                    _groundHit.distance = d + _groundDist;
                }
            }

            return theGroundIsMoving;
        }
    }

    /// <summary>
    /// How the character rotates.
    /// </summary>
    public enum CharacterRotateMode {
        None, Movement, Control, Manual,
    }

    /// <summary>
    /// How input is used to determine jump velocity.
    /// </summary>
    public enum CharacterJumpMode {
        Press, Charge,
    }

    /// <summary>
    /// How movement input is projected to create actual movement.
    /// </summary>
    public enum CharacterMovementProjectionMode {
        None, LocalY, GroundNormal, Gravity, Custom,
    }

    /// <summary>
    /// How input acceleration is applied to movement.
    /// </summary>
    public enum CharacterInputAccelerationMode {
        Instant, Force, MoveVelocity,
    }
}
