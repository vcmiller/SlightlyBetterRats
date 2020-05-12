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
    /// A hitscan projectile that instantly travels to the target upon firing.
    /// </summary>
    class InstantProjectile2D : Projectile2D {
        /// <summary>
        /// Layers that the projectile collides with.
        /// </summary>
        [Tooltip("Layers that the projectile collides with.")]
        public LayerMask hitMask = 1;

        /// <summary>
        /// Range of the projectile.
        /// </summary>
        [Tooltip("Range of the projectile.")]
        public float range = Mathf.Infinity;

        public override void Fire(Vector3 direction, bool align = true) {
            base.Fire(direction, align);
            RaycastHit2D hit;

            bool trig = Physics2D.queriesHitTriggers;
            Physics2D.queriesHitTriggers = hitsTriggers;

            if (hit = Physics2D.Raycast(transform.position, direction, range, hitMask)) {
                OnHitCollider2D(hit.collider, hit.point);
            } else {
                Destroy(gameObject, linger);
            }

            Physics2D.queriesHitTriggers = trig;
        }
    }
}

