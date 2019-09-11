using SBR.Serialization;
using System;
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
        public CapsuleCollider capsule { get; private set; }

        /// <summary>
        /// Rigidbody collider used by the character.
        /// </summary>
        new public Rigidbody rigidbody { get; private set; }

        /// <summary>
        /// Animator used for root motion.
        /// </summary>
        public Animator animator { get; private set; }

        /// <summary>
        /// Whether character is currently touching the ground.
        /// </summary>
        public bool grounded { get; private set; }

        /// <summary>
        /// Whether character is currently sliding down a steep slope.
        /// </summary>
        public bool sliding { get; private set; }

        /// <summary>
        /// Whether the character is currently jumping.
        /// </summary>
        public bool jumping { get; private set; }

        /// <summary>
        /// Current velocity due to gravity. Not affected by input, and clears when grounded.
        /// </summary>
        [NoOverrides]
        public Vector3 gravityVelocity { get; set; }

        /// <summary>
        /// Current velocity due to movement, which constantly moves towards input by acceleration.
        /// </summary>
        [NoOverrides]
        public Vector3 movementVelocity { get; set; }

        /// <summary>
        /// Rotation rates of the character as local euler angles.
        /// </summary>
        public Vector3 rotationRates { get; private set; }

        public Vector3 totalVelocity => gravityVelocity + movementVelocity;

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
        public Collider ground { get; private set; }

        /// <summary>
        /// World-space gravity acceleration on this Character.
        /// </summary>
        public Vector3 gravity => transform.TransformDirection(gravityDirection) * Mathf.Abs(Physics.gravity.y) * gravityScale;

        /// <summary>
        /// Current movement-based acceleration factoring in air control.
        /// </summary>
        public float actualMovementAcceleration => grounded ? movementAcceleration : movementAcceleration * airAccelerationMultiplier;

        public RaycastHit groundHit => _groundHit;
        private RaycastHit _groundHit;


        private Vector3 groundNormal;
        private Vector3 groundLastPos;
        private Vector3 groundHitLocalPos;

        private PhysicMaterial smoothAndSlippery;

        private Vector3 rootMotionMovement;
        private Quaternion rootMotionRotation = Quaternion.identity;
        private Vector3 rootMotionBonePos;
        private Quaternion rootMotionBoneRot = Quaternion.identity;
        private Quaternion rootMotionRotMod = Quaternion.identity;
        private float rootMotionBoneScale = 1.0f;

        private Quaternion targetRotation = Quaternion.identity;
        private bool isRotating;

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
        [Conditional("rotateMode", RotateMode.None, false)]
        public bool useSmoothRotation = true;

        /// <summary>
        /// Speed at which the character rotates. Only used if rotate mode is set to Movement.
        /// </summary>
        [Tooltip("Speed at which the character rotates. Only used if rotate mode is set to Movement.")]
        [Conditional("rotateMode", RotateMode.None, false)]
        public float rotationSpeed = 15;

        /// <summary>
        /// Whether to rotate on the local X axis.
        /// </summary>
        [Tooltip("Whether to rotate on the local X axis.")]
        [Conditional("rotateMode", RotateMode.None, false)]
        public bool rotateX = false;

        /// <summary>
        /// Whether to rotate on the local Y axis.
        /// </summary>
        [Tooltip("Whether to rotate on the local Y axis.")]
        [Conditional("rotateMode", RotateMode.None, false)]
        public bool rotateY = true;

        /// <summary>
        /// Whether to rotate on the local Z axis.
        /// </summary>
        [Tooltip("Whether to rotate on the local Z axis.")]
        [Conditional("rotateMode", RotateMode.None, false)]
        public bool rotateZ = false;

        /// <summary>
        /// If angle between rotation and target is greater than this, will start rotating.
        /// </summary>
        [Tooltip("If angle between rotation and target is greater than this, will start rotating.")]
        [Conditional("rotateMode", RotateMode.None, false)]
        public float maxTargetAngle = 0;

        /// <summary>
        /// If angle between rotation and target is less than this, will stop rotating.
        /// </summary>
        [Tooltip("If angle between rotation and target is less than this, will stop rotating.")]
        [Conditional("rotateMode", RotateMode.None, false)]
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
        /// Multiplier to apply to movement speed while not grounded.
        /// </summary>
        [Tooltip("Multiplier to apply to movement speed while not grounded.")]
        public float airAccelerationMultiplier = 0.5f;

        /// <summary>
        /// How to project the movement input vector.
        /// </summary>
        [Tooltip("How to project the movement input vector.")]
        public ProjectMovementMode projectMovement = ProjectMovementMode.GroundNormal;

        /// <summary>
        /// The maximum slope, in degrees, that the character can climb.
        /// </summary>
        [Tooltip("The maximum slope, in degrees, that the character can climb.")]
        public float maxGroundAngle = 45;

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
        /// If true, jumping will affect movement velocity rather than gravity velocity.
        /// </summary>
        [Tooltip("If true, jumping will affect movement velocity rather than gravity velocity.")]
        public bool jumpAffectsMovementVelocity = false;

        /// <summary>
        /// The value to multiply Physics.Gravity by.
        /// </summary>
        [Tooltip("The value to multiply Physics.Gravity by.")]
        public float gravityScale = 1;

        /// <summary>
        /// The value to multiply Physics.Gravity by.
        /// </summary>
        [Tooltip("The direction of gravity on the character.")]
        public Vector3 gravityDirection = Vector3.down;

        /// <summary>
        /// Distance for ground checks. Should be about 0.05 * capsule height.
        /// </summary>
        [Tooltip("Distance for ground checks. Should be about 0.05 * capsule height.")]
        public float groundDist = 0.1f;

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
        /// Bone transform that is considered the root. 
        /// Should be the same as the one specified in the animation settings.
        /// </summary>
        [Tooltip("Bone transform that is considered the root.")]
        public Transform rootMotionBone;

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
            None, Movement, Control
        }

        public enum ProjectMovementMode {
            None, LocalY, GroundNormal, Gravity
        }

        protected override void Awake() {
            base.Awake();

            capsule = GetComponent<CapsuleCollider>();
            rigidbody = GetComponent<Rigidbody>();
            animator = GetComponent<Animator>();

            rigidbody.useGravity = false;

            smoothAndSlippery = new PhysicMaterial();
            smoothAndSlippery.bounciness = 0;
            smoothAndSlippery.bounceCombine = PhysicMaterialCombine.Minimum;
            smoothAndSlippery.staticFriction = 0;
            smoothAndSlippery.dynamicFriction = 0;
            smoothAndSlippery.frictionCombine = PhysicMaterialCombine.Minimum;
            capsule.sharedMaterial = smoothAndSlippery;

            if (rootMotionBone) {
                rootMotionBonePos = rootMotionBone.localPosition;
                rootMotionBoneRot = rootMotionBone.localRotation;

                rootMotionRotMod = Quaternion.Inverse(Quaternion.Inverse(transform.rotation) * rootMotionBone.rotation);
                rootMotionBoneScale = rootMotionBone.lossyScale.x / transform.localScale.x;
            }

            Time.fixedDeltaTime = 1.0f / 60.0f;

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

        private void OnAnimatorMove() {
            if (animator && rootMotionBone) {
                rigidbody.isKinematic = true;

                Vector3 initPos = rigidbody.position;
                Quaternion initRot = rigidbody.rotation;

                animator.ApplyBuiltinRootMotion();

                rootMotionMovement = rigidbody.position - initPos;
                rootMotionRotation = Quaternion.Inverse(initRot) * rigidbody.rotation;

                rigidbody.position = initPos;
                rigidbody.rotation = initRot;

                rigidbody.isKinematic = false;
            }
        }

        private void UpdateJumping() {

            Vector3 relevantJumpVelocity;
            if (jumpAffectsMovementVelocity) {
                relevantJumpVelocity = movementVelocity;
            } else {
                relevantJumpVelocity = gravityVelocity;
            }

            if (jumping && transform.InverseTransformDirection(relevantJumpVelocity).y <= 0) {
                jumping = false;
                lastChannels.jump = false;
            }
        }

        private void LateUpdate() {
            UpdateGrounded();

            UpdateJumping();

            if (!receivingInput) {
                movementVelocity = Vector3.MoveTowards(movementVelocity, Vector3.zero, actualMovementAcceleration);
            }

            if (!grounded) {
                gravityVelocity += gravity * Time.deltaTime;
            } else {
                gravityVelocity = Vector3.zero;
            }

            rigidbody.velocity = Vector3.zero;

            if (rootMotionBone) {
                Vector3 v = rootMotionBone.transform.localPosition;

                if (useRootMotionXZ) {
                    v.x = rootMotionBonePos.x;
                    v.z = rootMotionBonePos.z;
                }

                if (useRootMotionY) {
                    v.y = rootMotionBonePos.y;
                }

                rootMotionBone.transform.localPosition = v;

                if (useRootMotionRotation) {
                    rootMotionBone.localRotation = rootMotionBoneRot;
                }
            }
        }

        public void MoveTargetRotationTowards(Quaternion rotation) {
            if (!(rotateX && rotateY && rotateZ)) {
                Vector3 euler = (Quaternion.Inverse(transform.rotation) * rotation).eulerAngles;
                if (!rotateX) euler.x = 0;
                if (!rotateY) euler.y = 0;
                if (!rotateZ) euler.z = 0;
                targetRotation = transform.rotation * Quaternion.Euler(euler);
            } else {
                targetRotation = rotation;
            }
        }

        private void DoRotationOutput(CharacterChannels channels) {
            if (rotateMode == RotateMode.Movement) {
                if (channels.movement.sqrMagnitude > 0) {
                    MoveTargetRotationTowards(Quaternion.LookRotation(channels.movement, transform.up));
                }
            } else if (rotateMode == RotateMode.Control) {
                MoveTargetRotationTowards(channels.rotation);
            }
        }

        private Vector3 ProjectMovementVelocity(Vector3 velocity) {
            Vector3 projectedTargetVel = velocity;
            if (projectMovement == ProjectMovementMode.GroundNormal && grounded) {
                projectedTargetVel = Vector3.ProjectOnPlane(velocity, groundNormal);
            } else if (projectMovement == ProjectMovementMode.LocalY ||
                (projectMovement == ProjectMovementMode.GroundNormal && !grounded)) {
                projectedTargetVel = Vector3.ProjectOnPlane(velocity, transform.up);
            } else if (projectMovement == ProjectMovementMode.Gravity) {
                projectedTargetVel = Vector3.ProjectOnPlane(velocity, gravityDirection);
            }
            return projectedTargetVel;
        }

        protected override void DoOutput(CharacterChannels channels) {
            DoRotationOutput(channels);

            Vector3 targetVel = channels.movement;
            Vector3 projectedTargetVel = ProjectMovementVelocity(targetVel);

            if (projectedTargetVel.sqrMagnitude > 0.001f) {
                projectedTargetVel = projectedTargetVel.normalized * targetVel.magnitude;
            }

            projectedTargetVel *= movementSpeed;
            
            if (sliding) {
                Vector3 n = Vector3.ProjectOnPlane(groundNormal, transform.up);
                n = -n.normalized;

                if (Vector3.Dot(projectedTargetVel, n) > 0) {
                    Vector3 bad = Vector3.Project(projectedTargetVel, n);
                    projectedTargetVel -= bad;
                }
            }

            float minMoveSpeed = movementSpeed * 0.01f;
            minMoveSpeed = minMoveSpeed * minMoveSpeed;

            bool wasMoving = movementVelocity.sqrMagnitude > minMoveSpeed;
            movementVelocity = Vector3.MoveTowards(movementVelocity, projectedTargetVel, actualMovementAcceleration * Time.deltaTime);
            bool isMoving = movementVelocity.sqrMagnitude > minMoveSpeed;

            if (wasMoving != isMoving) {
                Moving?.Invoke(isMoving);
            }

            if (grounded && channels.jump && enableInput && jumpVelocity.sqrMagnitude > 0) {
                Jumped?.Invoke();
                jumping = true;
                if (jumpAffectsMovementVelocity) {
                    movementVelocity += transform.TransformDirection(jumpVelocity);
                    gravityVelocity = Vector3.zero;
                } else {
                    gravityVelocity = transform.TransformDirection(jumpVelocity);
                }
            }
        }

        private void UpdateGrounded() {
            Vector3 pnt1, pnt2;
            float radius, height;

            capsule.GetCapsuleInfo(out pnt1, out pnt2, out radius, out height);

            var lastGround = ground;
            
            bool g = Physics.SphereCast(pnt2 + transform.up * groundDist, radius, -transform.up, out _groundHit, groundDist * 2, groundLayers, QueryTriggerInteraction.Ignore) && !jumping;

            grounded = g && Vector3.Angle(groundHit.normal, transform.up) <= maxGroundAngle;
            sliding = g && !grounded;

            if (g) {
                groundNormal = groundHit.normal;
            }

            if (grounded) {
                ground = groundHit.collider;

                if (lastGround != ground) {
                    groundHitLocalPos = ground.transform.InverseTransformPoint(groundHit.point);
                    groundLastPos = groundHit.point;
                }
            } else {
                ground = null;
            }

            if (lastGround != ground) {
                Grounded?.Invoke(ground);
            }
        }

        private void RotateTowardsTarget() {
            if (isRotating) {
                Quaternion q = rigidbody.rotation;
                if (useSmoothRotation) {
                    rigidbody.MoveRotation(Quaternion.Slerp(rigidbody.rotation, targetRotation, Time.fixedDeltaTime * rotationSpeed));
                } else {
                    rigidbody.MoveRotation(Quaternion.RotateTowards(rigidbody.rotation, targetRotation, Time.fixedDeltaTime * rotationSpeed));
                }
                rotationRates = MathUtil.NormalizeInnerAngles((Quaternion.Inverse(q) * rigidbody.rotation).eulerAngles) / Time.fixedDeltaTime;

                if (Quaternion.Angle(rigidbody.rotation, targetRotation) < minTargetAngle) {
                    isRotating = false;
                }
            } else {
                if (Quaternion.Angle(rigidbody.rotation, targetRotation) > maxTargetAngle) {
                    isRotating = true;
                }
                rotationRates = Vector3.zero;
            }
        }

        private void FixedUpdate() {
            Vector3 theGroundIsMoving = Vector3.zero;

            if (ground) {
                Vector3 curGroundHitPos = ground.transform.TransformPoint(groundHitLocalPos);

                if (moveWithPlatforms) theGroundIsMoving = curGroundHitPos - groundLastPos;
                groundLastPos = groundHit.point;
                groundHitLocalPos = ground.transform.InverseTransformPoint(groundHit.point);

                if (groundStickiness > 0 && !jumping) {
                    float d = Mathf.Lerp(_groundHit.distance - groundDist, 0, groundStickiness);
                    rigidbody.MovePosition(rigidbody.position - (rigidbody.rotation * Vector3.up) * (_groundHit.distance - d));
                    _groundHit.distance = d + groundDist;
                }
            }

            Vector3 rootMovement = Vector3.zero;

            if (rootMotionBone) {
                rootMovement = rootMotionMovement * rootMotionScale * rootMotionBoneScale;

                rootMovement = transform.InverseTransformVector(rootMovement);
                rootMovement = rootMotionRotMod * rootMovement;

                if (!useRootMotionY) {
                    rootMovement.y = 0;
                }

                if (!useRootMotionXZ) {
                    rootMovement.x = 0;
                    rootMovement.z = 0;
                }

                rootMovement = transform.TransformVector(rootMovement);

                rootMotionMovement = Vector3.zero;

                if (useRootMotionRotation) {
                    rigidbody.MoveRotation(rootMotionRotMod * rootMotionRotation * Quaternion.Inverse(rootMotionRotMod) * rigidbody.rotation);
                }
            }

            rigidbody.MovePosition(rigidbody.position + (gravityVelocity + movementVelocity) * Time.fixedDeltaTime + theGroundIsMoving + rootMovement);

            if (rotateMode != RotateMode.None && enableInput) {
                RotateTowardsTarget();
            }
        }

        private void FlattenVelocity(Vector3 normal) {
            if (Vector3.Dot(gravityVelocity, normal) < 0) {
                gravityVelocity = Vector3.ProjectOnPlane(gravityVelocity, normal);
            }

            if (Vector3.Dot(movementVelocity, normal) < 0) {
                movementVelocity = ProjectMovementVelocity(Vector3.ProjectOnPlane(movementVelocity, normal));
            }
        }

        private void OnCollisionStay(Collision collision) {
            FlattenVelocity(collision.GetContact(0).normal);
        }
    }
}