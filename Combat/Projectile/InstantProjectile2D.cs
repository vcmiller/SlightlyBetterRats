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

