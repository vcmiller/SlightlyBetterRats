using UnityEngine;

namespace SBR {
    /// <summary>
    /// A hitscan projectile that instantly travels to the target upon firing.
    /// </summary>
    class InstantProjectile : Projectile {
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
            RaycastHit hit;

            if (Physics.Raycast(transform.position, direction, out hit, range, hitMask, triggerInteraction)) {
                OnHitCollider(hit.collider, hit.point);
            } else {
                Destroy(gameObject, linger);
            }
        }
    }
}

