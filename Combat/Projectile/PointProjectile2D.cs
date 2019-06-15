using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SBR {
    /// <summary>
    /// A projectile that detects collisions using Physics2D.Raycast.
    /// </summary>
    public class PointProjectile2D : Projectile2D {
        /// <summary>
        /// Layers that the projectile collides with.
        /// </summary>
        [Tooltip("Layers that the projectile collides with.")]
        public LayerMask hitMask = 1;

        /// <summary>
        /// Extra length to raycast each frame, to compensate for other objects' velocity.
        /// </summary>
        [Tooltip("Extra length to raycast each frame, to compensate for other objects' velocity.")]
        public float offset = 0;

        private void Update() {
            if (velocity.sqrMagnitude > 0) {
                Vector3 oldPosition = transform.position;
                transform.position += velocity * Time.deltaTime;

                RaycastHit2D hit;

                bool trig = Physics2D.queriesHitTriggers;
                Physics2D.queriesHitTriggers = hitsTriggers;

                if (hit = Physics2D.Linecast(oldPosition - velocity.normalized * offset, transform.position, hitMask)) {
                    OnHitCollider2D(hit.collider, hit.point);
                }

                Physics2D.queriesHitTriggers = trig;
            }
        }
    }
}