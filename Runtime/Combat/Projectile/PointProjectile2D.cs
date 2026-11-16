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

namespace SBR {
    /// <summary>
    /// A projectile that detects collisions using Physics2D.Raycast.
    /// </summary>
    public class PointProjectile2D : Projectile2D {
        /// <summary>
        /// Layers that the projectile collides with.
        /// </summary>
        [FormerlySerializedAs("hitMask")]
        [Tooltip("Layers that the projectile collides with.")]
        public LayerMask _hitMask = 1;

        /// <summary>
        /// Extra length to raycast each frame, to compensate for other objects' velocity.
        /// </summary>
        [FormerlySerializedAs("offset")]
        [Tooltip("Extra length to raycast each frame, to compensate for other objects' velocity.")]
        public float _offset = 0;

        protected override void Update() {
            base.Update();
            if (Velocity.sqrMagnitude > 0) {
                Vector3 oldPosition = transform.position;
                transform.position += Velocity * Time.deltaTime;


                bool trig = Physics2D.queriesHitTriggers;
                Physics2D.queriesHitTriggers = HitsTriggers;

                RaycastHit2D hit =
                    Physics2D.Linecast(oldPosition - Velocity.normalized * _offset, transform.position, _hitMask);
                if (hit) {
                    OnHitCollider2D(hit.collider, hit.point, hit.normal);
                }

                Physics2D.queriesHitTriggers = trig;
            }
        }
    }
}
