// MIT License
// 
// Copyright (c) 2020 Vincent Miller
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

namespace SBR {
    /// <summary>
    /// A projectile that uses a Collider to determine when it hits an object.
    /// Should have a Rigidbody with isKinematic set to true in order to work properly.
    /// </summary>
    public class ColliderProjectile : Projectile {
        private Rigidbody _rigidbody;

        [SerializeField] private bool _kinematicOnHit;

        public override Vector3 velocity {
            get => base.velocity;
            set {
                base.velocity = value;
                if (_rigidbody) _rigidbody.velocity = value;
            }
        }

        public override void Fire(Vector3 direction, bool align = true) {
            _rigidbody = GetComponent<Rigidbody>();
            if (_rigidbody) _rigidbody.isKinematic = false;
            base.Fire(direction, align);
        }

        protected override void Update() {
            base.Update();
            
            if (_rigidbody) {
                velocity = _rigidbody.velocity;
            } else {
                transform.position += velocity * Time.deltaTime;
            }
        }

        private void OnCollisionEnter(Collision collision) {
            var hit = collision.contacts[0];
            OnHitCollider(collision.collider, hit.point);
        }

        private void OnTriggerEnter(Collider other) {
            OnHitCollider(other, transform.position);
        }

        protected override void OnHitObject(Transform col, Vector3 position) {
            base.OnHitObject(col, position);
            if (_kinematicOnHit && _rigidbody) {
                _rigidbody.isKinematic = true;
            }
        }
    }
}