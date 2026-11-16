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
using Infohazard.Core;
using UnityEngine;

namespace SBR {
    /// <summary>
    /// A projectile that uses a Collider to determine when it hits an object.
    /// Should have a Rigidbody with isKinematic set to true in order to work properly.
    /// </summary>
    public class ColliderProjectile : Projectile {
        private Rigidbody _rigidbody;

        [SerializeField] private bool _kinematicOnHit;

        public override Vector3 Velocity {
            get => base.Velocity;
            set {
                base.Velocity = value;
                if (_rigidbody && !_rigidbody.isKinematic) _rigidbody.SetLinearVelocity(value);
            }
        }

        public override void Fire(GameObject creator, Vector3 direction, bool align = true, object method = null) {
            _rigidbody = GetComponent<Rigidbody>();
            if (_rigidbody) _rigidbody.isKinematic = false;
            base.Fire(creator, direction, align, method);
        }

        protected override void Update() {
            base.Update();

            if (_rigidbody && !_rigidbody.isKinematic) {
                Velocity = _rigidbody.GetLinearVelocity();
            } else {
                transform.position += Velocity * Time.deltaTime;
            }
        }

        private void FixedUpdate() {
            if (_rigidbody && _rigidbody.isKinematic) {
                _rigidbody.position += Velocity * Time.fixedDeltaTime;
            }
        }

        private void OnCollisionEnter(Collision collision) {
            ContactPoint hit = collision.contacts[0];
            OnHitCollider(collision.collider, hit.point, hit.normal);
        }

        private void OnTriggerEnter(Collider other) {
            OnHitCollider(other, transform.position, Velocity.normalized);
        }

        protected override void OnHitObject(Transform col, Vector3 position, Vector3 normal) {
            base.OnHitObject(col, position, normal);
            if (_kinematicOnHit && _rigidbody) {
                _rigidbody.isKinematic = true;
            }
        }
    }
}
