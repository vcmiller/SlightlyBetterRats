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
    /// A projectile that detects collisions using Physics.Raycast.
    /// </summary>
    public class PointProjectile : Projectile {
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

        /// <summary>
        /// Radius of the projectile. If > 0, movement will use a SphereCast (otherwise LineCast).
        /// </summary>
        [FormerlySerializedAs("radius")]
        [Tooltip("Radius of the projectile. If > 0, movement will use a SphereCast (otherwise LineCast).")]
        public float _radius;

        /// <summary>
        /// Maximum number of objects that can be hit in a single step.
        /// </summary>
        [FormerlySerializedAs("maxHits")]
        [Tooltip("Maximum number of objects that can be hit in a single step.")]
        public int _maxHits = 8;

        protected override void Update() {
            base.Update();
            Vector3 dir = Velocity.normalized;
            if (Velocity.sqrMagnitude > 0) {
                Vector3 position = transform.position - dir * _offset;
                float distance = Velocity.magnitude * Time.deltaTime + _offset;

                while (distance > 0) {
                    bool didHit;
                    RaycastHit hit;
                    if (_radius <= 0) {
                        didHit = Physics.Raycast(position, dir, out hit, distance, _hitMask, _triggerInteraction);
                    } else {
                        didHit = Physics.SphereCast(position, _radius, dir, out hit, distance, _hitMask, _triggerInteraction);
                    }

                    if (didHit) {
                        position += dir * (hit.distance + 0.01f);
                        distance -= hit.distance;
                        if (OnHitCollider(hit.collider, hit.point, hit.normal) && (_stopOnHit || _destroyOnHit)) {
                            break;
                        }
                    } else {
                        position += dir * distance;
                        distance = 0;
                    }
                }

                transform.position = position;
            }
        }
    }
}
