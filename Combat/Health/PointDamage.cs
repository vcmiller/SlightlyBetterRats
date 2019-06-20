using UnityEngine;

namespace SBR {
    public class PointDamage : Damage {
        /// <summary>
        /// Create a new PointDamage instance.
        /// </summary>
        /// <param name="amount">Amount of damage.</param>
        /// <param name="point">Location of the damage.</param>
        /// <param name="direction">Direction of the damage.</param>
        /// <param name="force">Force applied by the bullet.</param>
        public PointDamage(float amount, Vector3 point, Vector3 direction, float force) : base(amount) {
            this.point = point;
            this.direction = direction.normalized;
            this.force = force;
        }

        /// <summary>
        /// Location at which the damage is applied.
        /// </summary>
        public Vector3 point;

        /// <summary>
        /// Direction of the damage (such as the normalized velocity of a bullet).
        /// </summary>
        public Vector3 direction;

        /// <summary>
        /// Force that is applied by the bullet in the given direction.
        /// </summary>
        public float force;
    }

}