using UnityEngine;

namespace SBR {
    public class PointDamage : Damage {
        /// <summary>
        /// Create a new PointDamage instance.
        /// </summary>
        /// <param name="amount">Amount of damage.</param>
        /// <param name="point">Location of the damage.</param>
        /// <param name="impulse">Impulse done by the damage.</param>
        public PointDamage(float amount, Vector3 point, Vector3 impulse) : base(amount) {
            this.point = point;
            this.impulse = impulse;
        }

        /// <summary>
        /// Location at which the damage is applied.
        /// </summary>
        public Vector3 point;

        /// <summary>
        /// Direction of the damage (such as the impact velocity of a bullet).
        /// </summary>
        public Vector3 impulse;
    }

}