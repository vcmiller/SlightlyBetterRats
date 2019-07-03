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

        /// <summary>
        /// Maximum number of objects that can be hit in a single step.
        /// </summary>
        [Tooltip("Maximum number of objects that can be hit in a single step.")]
        public int maxHits = 8;

        private RaycastHit[] hitArray;

        private void Awake() {
            hitArray = new RaycastHit[maxHits];
        }

        protected virtual void Update() {
            Vector3 dir = velocity.normalized;
            if (velocity.sqrMagnitude > 0) {
                Vector3 position = transform.position - dir * offset;
                float distance = velocity.magnitude + offset;

                while (distance > 0) {
                    bool didHit;
                    RaycastHit hit;
                    if (radius <= 0) {
                        didHit = Physics.Raycast(position, dir, out hit, distance, hitMask, triggerInteraction);
                    } else {
                        didHit = Physics.SphereCast(position, radius, dir, out hit, distance, hitMask, triggerInteraction);
                    }

                    if (didHit) {
                        position += dir * (hit.distance + 0.01f);
                        distance -= hit.distance;
                        if (OnHitCollider(hit.collider, hit.point)) {
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