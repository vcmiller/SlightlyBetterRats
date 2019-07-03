using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SBR {
    /// <summary>
    /// Extension of IDamageable that allows damage to be received which is dealt to any child object.
    /// </summary>
    public interface IParentDamageable : IDamageable { }

    /// <summary>
    /// Interface for components that can be damaged by the GameObject.Damage and Component.Damage extension methods.
    /// For an object to receive damage, the damageable component must be on that exact object, not a parent or child.
    /// </summary>
    public interface IDamageable {

        /// <summary>
        /// Apply damage to the object.
        /// </summary>
        /// <param name="dmg">The damage to apply.</param>
        /// <returns>The actual damage amount dealt.</returns>
        float Damage(Damage dmg);
    }

    public static class DamageableExtensions {
        /// <summary>
        /// Damage the GameObject if it has a Damageable component.
        /// </summary>
        /// <param name="obj">The GameObject to damage.</param>
        /// <param name="dmg">The damage object.</param>
        /// <returns>The actual damage amount dealt.</returns>
        public static float Damage(this GameObject obj, Damage dmg) {
            if (obj.TryGetComponent(out IDamageable d1)) {
                return d1.Damage(dmg);
            } else if (obj.TryGetComponentInParent(out IParentDamageable d2)) {
                return d2.Damage(dmg);
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