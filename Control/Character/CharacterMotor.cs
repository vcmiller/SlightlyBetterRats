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
        /// Whether the character jumped this frame.
        /// </summary>
        public bool jumpedThisFrame { get; private set; }

        /// <summary>
        /// Whether the character is currently jumping.
        /// </summary>
        public bool jumping { get; private set; }

        /// <summary>
        /// Whether to allow the character to control its air velocity.
        /// </summary>
        public bool enableAirControl { get; set; }

        /// <summary>
        /// Current velocity of the character. Use this to add forces.
        /// </summary>
        public Vector3 velocity { get; set; }

        /// <summary>
        /// Current ground collider if grounded is true.
        /// </summary>
        public Collider ground { get; private set; }

        private RaycastHit groundHit;

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

        /// <summary>
        /// Layers that block the character. Should be the same as the layers the collider interacts with.
        /// </summary>
        [Header("General")]
        [Tooltip("Layers that block the character. Should be the same as the layers the collider interacts with.")]
        public LayerMask groundLayers = 1;

        /// <summary>
        /// How the character rotates in relation to its movement.
        /// </summary>
        [Tooltip("How the character rotates in relation to its movement.")]
        public RotateMode rotateMode = RotateMode.None;

        /// <summary>
        /// Speed at which the character rotates. Only used if rotate mode is set to Movement.
        /// </summary>
        [Tooltip("Speed at which the character rotates. Only used if rotate mode is set to Movement.")]
        public float rotationSpeed = 360;
        
        /// <summary>
        /// The max walk speed of the character.
        /// </summary>
        [Tooltip("The max walk speed of the character.")]
        [Header("Walking")]
        public float walkSpeed = 5;

        /// <summary>
        /// The walking (ground) acceleration of the character.
        /// </summary>
        [Tooltip("The walking (ground) acceleration of the character.")]
        public float walkAcceleration = 25;

        /// <summary>
        /// The maximum slope, in degrees, that the character can climb.
        /// </summary>
        [Tooltip("The maximum slope, in degrees, that the character can climb.")]
        public float maxSlope = 45;

        /// <summary>
        /// Whether the player's movement should be aligned with the slope they are standing on.
        /// </summary>
        [Tooltip("Whether the player's movement should be aligned with the slope they are standing on.")]
        public bool keepOnSlope = true;

        /// <summary>
        /// Whether the player will automatically stay on moving platforms.
        /// </summary>
        [Tooltip("Whether the player will automatically stay on moving platforms.")]
        public bool moveWithPlatforms = true;

        /// <summary>
        /// Whether to apply root motion from animations in the XZ plane.
        /// </summary>
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
        /// The speed at which the character jumps.
        /// </summary>
        [Header("Jumping")]
        [Tooltip("The speed at which the character jumps.")]
        public float jumpSpeed = 4;

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

        /// <summary>
        /// Air control multiplier (air acceleration is Air Control * Walk Acceleration.
        /// </summary>
        [Header("Movement: Falling")]
        [Tooltip("Air control multiplier (air acceleration is Air Control * Walk Acceleration.")]
        public float airControl = 0.5f;

        /// <summary>
        /// Current rate at which the character can accelerate, if trying to move at max speed.
        /// </summary>
        public float acceleration {
            get {
                float accel = walkAcceleration;
                if (!grounded) {
                    if (enableAirControl) {
                        accel *= airControl;
                    } else {
                        accel = 0;
                    }
                }
                return accel;
            }
        }
        
        public enum RotateMode {
            None, Movement, Control
        }

        protected override void Awake() {
            base.Awake();

            capsule = GetComponent<CapsuleCollider>();
            rigidbody = GetComponent<Rigidbody>();
            animator = GetComponent<Animator>();
            enableAirControl = true;

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

        private void LateUpdate() {
            UpdateGrounded();

            if (!receivingInput) {
                velocity = Vector3.MoveTowards(velocity, Vector3.Project(velocity, transform.up), acceleration);
            }

            if (!grounded) {
                velocity += Physics.gravity * gravityScale * Time.deltaTime;
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

        protected override void DoOutput(CharacterChannels channels) {
            Vector3 move = Vector3.zero;
            
            move = Vector3.ProjectOnPlane(channels.movement, transform.up) * walkSpeed;

            if (rotateMode == RotateMode.Movement) {
                Vector3 v = channels.movement;
                v.y = 0;
                if (v.sqrMagnitude > 0) {
                    v = v.normalized;
                    Vector3 axis = Vector3.Cross(transform.forward, v);
                    if (Mathf.Approximately(axis.sqrMagnitude, 0)) {
                        axis = Vector3.up;
                    }

                    float angle = Vector3.Angle(transform.forward, v);
                    float amount = Mathf.Min(angle, Time.deltaTime * rotationSpeed);
                    transform.Rotate(axis, amount, Space.World);
                }
            } else if (rotateMode == RotateMode.Control) {
                transform.eulerAngles = new Vector3(0, channels.rotation.eulerAngles.y, 0);
            }
            
            if (sliding) {
                Vector3 n = Vector3.ProjectOnPlane(groundNormal, transform.up);
                n = -n.normalized;

                if (Vector3.Dot(move, n) > 0) {
                    Vector3 bad = Vector3.Project(move, n);
                    move -= bad;
                }
            } else if (grounded && keepOnSlope) {
                move = Vector3.ProjectOnPlane(move, groundNormal).normalized * move.magnitude;
            }
            
            Vector3 targetVel = move;
            if (!grounded) {
                targetVel += Vector3.Project(velocity, Physics.gravity);
            }
            velocity = Vector3.MoveTowards(velocity, targetVel, acceleration * Time.deltaTime);

            jumpedThisFrame = false;
            if (grounded && channels.jump && enableInput) {
                jumpedThisFrame = true;
                jumping = true;
                velocity = Vector3.ProjectOnPlane(velocity, transform.up) + transform.up * jumpSpeed;
            }

            if (Vector3.Dot(velocity, transform.up) <= 0) {
                jumping = false;
                channels.jump = false;
            }
        }

        private void UpdateGrounded() {
            Vector3 pnt1, pnt2;
            float radius, height;
            
            capsule.GetCapsuleInfo(out pnt1, out pnt2, out radius, out height);

            var lastGround = ground;
            
            bool g = Physics.SphereCast(pnt2 + transform.up * groundDist, radius, -transform.up, out groundHit, groundDist * 2, groundLayers, QueryTriggerInteraction.Ignore) && !jumping;

            grounded = g && Vector3.Angle(groundHit.normal, transform.up) <= maxSlope;
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
        }

        private void FixedUpdate() {
            Vector3 theGroundIsMoving = Vector3.zero;

            if (ground) {
                Vector3 curGroundHitPos = ground.transform.TransformPoint(groundHitLocalPos);

                if (moveWithPlatforms) theGroundIsMoving = curGroundHitPos - groundLastPos;
                groundLastPos = groundHit.point;
                groundHitLocalPos = ground.transform.InverseTransformPoint(groundHit.point);
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

            rigidbody.MovePosition(rigidbody.position + velocity * Time.fixedDeltaTime + theGroundIsMoving + rootMovement);
        }

        private void OnCollisionStay(Collision other) {
            var normal = other.contacts[0].normal;

            if (Vector3.Dot(velocity, normal) >= 0) return;

            velocity += Vector3.Project(-velocity, normal);
        }
    }
}