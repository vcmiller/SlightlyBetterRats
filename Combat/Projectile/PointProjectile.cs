using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SBR {
    /// <summary>
    /// A projectile that detects collisions using Physics.Raycast.
    /// </summary>
    public class PointProjectile : Projectile {
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

        /// <summary>
        /// Radius of the projectile. If > 0, movement will use a SphereCast (otherwise LineCast).
        /// </summary>
        [Tooltip("Radius of the projectile. If > 0, movement will use a SphereCast (otherwise LineCast).")]
        public float radius;

        protected virtual void Update() {
            if (velocity.sqrMagnitude > 0) {
                Vector3 oldPosition = transform.position;
                transform.position += velocity * Time.deltaTime;
                
                if (radius <= 0) {
                    if (Physics.Linecast(oldPosition - velocity.normalized * offset, 
                        transform.position,  out var hit,  hitMask,  triggerInteraction)) {

                        OnHitCollider(hit.collider, hit.point);
                    }
                } else {
                    var dir = transform.position - oldPosition;
                    if (Physics.SphereCast(oldPosition - velocity.normalized * offset, radius, 
                        dir, out var hit, dir.magnitude + offset, hitMask, triggerInteraction)) {

                        OnHitCollider(hit.collider, hit.point);
                    }
                }
            }
        }
    }
}