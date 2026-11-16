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

using UnityEngine;
using UnityEngine.Serialization;

/**
 * Motor class for a humanoid character. Enables movement and collision without the use of a Rigidbody.
 * However, if you want to get trigger/collision events for objects that don't have a Rigidbody, you
 * need to put a Rigidbody on this object and make it kinematic.
 */
namespace SBR {
    [RequireComponent(typeof(BoxCollider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class CharacterMotor2D : Motor<CharacterChannels> {
        public BoxCollider2D Box { get; private set; }
        new public Rigidbody2D Rigidbody { get; private set; }

        public bool Grounded { get; private set; }
        public bool Sliding { get; private set; }
        public bool JumpedThisFrame { get; private set; }
        public bool Jumping { get; private set; }
        public bool EnableAirControl { get; set; }

        public Collider2D Ground { get; private set; }

        private Vector2 _groundNormal;
        private Vector2 _groundLastPos;

        private PhysicsMaterial2D _smoothAndSlippery;

        [FormerlySerializedAs("velocity"),HideInInspector]
        public Vector2 _velocity;

        [FormerlySerializedAs("groundLayers"),Header("General")]
        [Tooltip("Layers that block the character. Should be the same as the layers the collider interacts with.")]
        public LayerMask _groundLayers = 1;

        [FormerlySerializedAs("walkSpeed"),Header("Movement: Walking")]
        [Tooltip("The max walk speed of the character.")]
        public float _walkSpeed = 10;

        [FormerlySerializedAs("walkAcceleration"),Tooltip("The walking (ground) acceleration of the character.")]
        public float _walkAcceleration = 50;

        [FormerlySerializedAs("maxSlope"),Tooltip("The maximum slope, in degrees, that the character can climb.")]
        public float _maxSlope = 45;

        [FormerlySerializedAs("keepOnSlope"),Tooltip("Whether the player's movement should be aligned with the slope they are standing on.")]
        public bool _keepOnSlope = true;

        [FormerlySerializedAs("moveWithPlatforms"),Tooltip("Whether the player will automatically stay on moving platforms.")]
        public bool _moveWithPlatforms = true;
        
        [FormerlySerializedAs("jumpSpeed"),Header("Jumping")]
        [Tooltip("The speed at which the character jumps.")]
        public float _jumpSpeed = 10;

        [FormerlySerializedAs("enableJumpCancel"),Tooltip("Whether releasing the jump button should immediately cancel the jump.")]
        public bool _enableJumpCancel = true;

        [FormerlySerializedAs("gravityScale"),Tooltip("The value to multiply Physics.Gravity by.")]
        public float _gravityScale = 2;

        [FormerlySerializedAs("groundDist"),Tooltip("Distance to query when checking if player is on the ground.")]
        public float _groundDist = 0.1f;

        [FormerlySerializedAs("airControl"),Header("Movement: Falling")]
        [Tooltip("Air control multiplier (air acceleration is Air Control * Walk Acceleration.")]
        public float _airControl = 0.5f;

        protected override void Awake() {
            base.Awake();

            Box = GetComponent<BoxCollider2D>();
            Rigidbody = GetComponent<Rigidbody2D>();
            EnableAirControl = true;

            Rigidbody.gravityScale = 0;

            _smoothAndSlippery = new PhysicsMaterial2D();
            _smoothAndSlippery.bounciness = 0;
            _smoothAndSlippery.friction = 0;
            Box.sharedMaterial = _smoothAndSlippery;
            
            Time.fixedDeltaTime = 1.0f / 60.0f;
        }

        protected override void DoOutput(CharacterChannels channels) {
            Vector2 move = Vector2.zero;

            if (EnableInput) {
                move = Vector3.Project(channels.Movement, transform.right) * _walkSpeed;
            }

            //if (body.isGrounded) {
            float accel = _walkAcceleration;
            if (!Grounded) {
                if (EnableAirControl) {
                    accel *= _airControl;
                } else {
                    accel = 0;
                }
            }

            if (Sliding) {
                Vector2 n = Vector3.Project(_groundNormal, transform.right);
                n = -n.normalized;

                if (Vector2.Dot(move, n) > 0) {
                    Vector2 bad = Vector3.Project(move, n);
                    move -= bad;
                }
            } else if (Grounded && _keepOnSlope) {
                Vector2 gOrtho = new Vector2(_groundNormal.y, -_groundNormal.x);
                move = Vector3.Project(move, gOrtho).normalized * move.magnitude;
            }
            
            Vector2 targetVel = move;
            if (!Grounded) {
                targetVel += (Vector2)Vector3.Project(_velocity, Physics2D.gravity);
            }
            _velocity = Vector2.MoveTowards(_velocity, targetVel, accel * Time.deltaTime);

            JumpedThisFrame = false;
            if (Grounded && channels.Jump) {
                JumpedThisFrame = true;
                Jumping = true;
                _velocity = Vector3.Project(_velocity, transform.right) + transform.up * _jumpSpeed;
            }

            if (Vector2.Dot(_velocity, transform.up) <= 0) {
                Jumping = false;
                channels.Jump = false;
            }
            
            if (Jumping && !channels.Jump && _enableJumpCancel) {
                Jumping = false;
                _velocity.y = 0;
            }
        }

        private void UpdateGrounded() {
            Vector2 c = Box.transform.TransformPoint(Box.offset);
            Vector2 s = Box.transform.TransformVector(Box.size);
            float a = Box.transform.eulerAngles.z;

            Collider2D lastGround = Ground;

            bool tr = Physics2D.queriesHitTriggers;
            bool st = Physics2D.queriesStartInColliders;
            Physics2D.queriesHitTriggers = false;
            Physics2D.queriesStartInColliders = false;
            RaycastHit2D hit = Physics2D.BoxCast(c, s, a, -transform.up, _groundDist, _groundLayers);
            Physics2D.queriesHitTriggers = tr;
            Physics2D.queriesStartInColliders = st;

            bool g = hit && !Jumping;
            
            Grounded = g && Vector2.Angle(hit.normal, transform.up) <= _maxSlope;
            Sliding = g && !Grounded;

            if (g) {
                _groundNormal = hit.normal;
            }

            if (Grounded) {
                Ground = hit.collider;

                if (lastGround != Ground) {
                    _groundLastPos = Ground.transform.position;
                }
            } else {
                Ground = null;
            }
        }
        
        private void Update() {
            UpdateGrounded();
            
            if (!Grounded) {
                _velocity += Physics2D.gravity * _gravityScale * Time.deltaTime;
            }

            Rigidbody.linearVelocity = Vector2.zero;
        }

        private void FixedUpdate() {
            Vector2 theGroundIsMoving = Vector2.zero;

            if (Ground) {
                if (_moveWithPlatforms) theGroundIsMoving = (Vector2)Ground.transform.position - _groundLastPos;
                _groundLastPos = Ground.transform.position;
            }
            
            Rigidbody.MovePosition(Rigidbody.position + _velocity * Time.fixedDeltaTime + theGroundIsMoving);
        }

        private void OnCollisionStay2D(Collision2D other) {
            Vector2 normal = other.contacts[0].normal;
            print(normal);

            if (Vector2.Dot(_velocity, normal) >= 0) return;

            _velocity += (Vector2)Vector3.Project(-_velocity, normal);
        }
    }
}
