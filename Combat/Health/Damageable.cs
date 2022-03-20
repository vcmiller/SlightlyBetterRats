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
            if (dmg is PointDamage pointDamage && pointDamage.Force > 0) {
                Vector3 impulse = pointDamage.Direction * pointDamage.Amount;
                
                var rb = obj.GetComponentInParent<Rigidbody>();
                if (rb) rb.AddForce(impulse, ForceMode.Impulse);
                var rb2d = obj.GetComponentInParent<Rigidbody2D>();
                if (rb2d) rb2d.AddForce(impulse, ForceMode2D.Impulse);
            } 
            
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