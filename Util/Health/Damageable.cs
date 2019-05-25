using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SBR {
    public abstract class Damageable : MonoBehaviour {
        /// <summary>
        /// Apply damage to the object.
        /// </summary>
        /// <param name="amount">The amount of damage.</param>
        /// <returns>The actual damage amount dealt.</returns>
        public float Damage(float amount) {
            return Damage(new Damage(amount));
        }

        /// <summary>
        /// Apply damage to the object.
        /// </summary>
        /// <param name="dmg">The damage to apply.</param>
        /// <returns>The actual damage amount dealt.</returns>
        public abstract float Damage(Damage dmg);
    }

    public static class DamageableExtensions {
        /// <summary>
        /// Damage the GameObject if it has a Damageable component.
        /// </summary>
        /// <param name="obj">The GameObject to damage.</param>
        /// <param name="dmg">The damage object.</param>
        /// <returns>The actual damage amount dealt.</returns>
        public static float Damage(this GameObject obj, Damage dmg) {
            Damageable d = obj.GetComponentInParent<Damageable>();
            if (d) {
                return d.Damage(dmg);
            } else {
                return 0;
            }
        }

        /// <summary>
        /// Damage the GameObject if it has a Damageable component.
        /// </summary>
        /// <param name="obj">The GameObject to damage.</param>
        /// <param name="amount">The amount of damage.</param>
        /// <returns>The actual damage amount dealt.</returns>
        public static float Damage(this GameObject obj, float amount) {
            return obj.Damage(new Damage(amount));
        }

        /// <summary>
        /// Damage the GameObject if it has a Damageable component.
        /// </summary>
        /// <param name="cmp">The Component of the GameObject to damage.</param>
        /// <param name="dmg">The damage object.</param>
        /// <returns>The actual damage amount dealt.</returns>
        public static float Damage(this Component cmp, Damage dmg) {
            return cmp.gameObject.Damage(dmg);
        }

        /// <summary>
        /// Damage the GameObject if it has a Damageable component.
        /// </summary>
        /// <param name="obj">The Component of the GameObject to damage.</param>
        /// <param name="amount">The amount of damage.</param>
        /// <returns>The actual damage amount dealt.</returns>
        public static float Damage(this Component cmp, float amount) {
            return cmp.gameObject.Damage(amount);
        }
    }
}