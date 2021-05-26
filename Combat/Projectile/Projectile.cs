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

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SBR {
    /// <summary>
    /// Base class for Projectiles.
    /// </summary>
    public class Projectile : MonoBehaviour {
        /// <summary>
        /// Speed that the projectile is fired at.
        /// </summary>
        [Tooltip("Speed that the projectile is fired at.")]
        public float launchSpeed;

        /// <summary>
        /// How much damage to do if the projectile hits an object with a Health component.
        /// </summary>
        [Tooltip("How much damage to do if the projectile hits an object with a Health component.")]
        public float damage;

        /// <summary>
        /// How much force to apply if the projectile hits an object with a Rigidbody.
        /// </summary>
        [Tooltip("How much force to apply if the projectile hits an object with a Rigidbody.")]
        public float impactForce;

        /// <summary>
        /// Whether the projectile should hit triggers.
        /// </summary>
        [Tooltip("Whether the projectile should hit triggers.")]
        public QueryTriggerInteraction triggerInteraction;

        /// <summary>
        /// Whether the projectile should be able to hit objects before it is fired.
        /// </summary>
        [Tooltip("Whether the projectile should be able to hit objects before it is fired.")]
        public bool hitsIfNotFired;

        /// <summary>
        /// How long the projectile should last after it hits, if destroyOnHit is true.
        /// </summary>
        [Tooltip("How long the projectile should last after it hits, if destroyOnHit is true.")]
        public float linger;

        /// <summary>
        /// Multiplier for Physics.gravity or Physics2D.gravity.
        /// </summary>
        [Tooltip("Multiplier for Physics.gravity or Physics2D.gravity.")]
        public float gravity;

        /// <summary>
        /// Whether the projectile should destroy its GameObject on impact.
        /// </summary>
        [Tooltip("Whether the projectile should destroy its GameObject on impact.")]
        public bool destroyOnHit = true;

        /// <summary>
        /// Whether the projectile should stop moving when it hits an object.
        /// </summary>
        [Tooltip("Whether the projectile should stop moving when it hits an object.")]
        public bool stopOnHit = true;

        /// <summary>
        /// Prefab to spawn on impact, such as an explosion.
        /// </summary>
        [Tooltip("Prefab to spawn on impact, such as an explosion.")]
        public GameObject impactPrefab;

        /// <summary>
        /// Whether to parent the spawned impact object to the hit object.
        /// </summary>
        [Tooltip("Whether to parent the spawned impact object to the hit object.")]
        public bool parentImpactObject;

        /// <summary>
        /// Sound to play on impact.
        /// </summary>
        [Tooltip("Sound to play on impact.")]
        public AudioParameters impactSound;

        /// <summary>
        /// Current velocity of the projectile.
        /// </summary>
        public Vector3 velocity { get; set; }

        /// <summary>
        /// Whether the projectile has been fired.
        /// </summary>
        public bool fired { get; protected set; }
        
        public float damageMultiplier { get; set; }

        /// <summary>
        /// Invoked when the projectile collides with an object.
        /// </summary>
        public event Action<GameObject, Vector3> HitObject;
        
        protected virtual bool hitsTriggers => triggerInteraction == QueryTriggerInteraction.Collide ||
                    (triggerInteraction == QueryTriggerInteraction.UseGlobal && Physics.queriesHitTriggers);
        
        protected virtual Vector3 gravityVector => gravity * Physics.gravity;

        /// <summary>
        /// Fire the projectile in the direction it is currently facing.
        /// </summary>
        public virtual void Fire() {
            Fire(transform.forward, false);
        }

        /// <summary>
        /// Fire the projectile in a given direction.
        /// </summary>
        /// <param name="direction">Direction in which to fire.</param>
        /// <param name="align">Whether to orient the projectile to the given direction.</param>
        public virtual void Fire(Vector3 direction, bool align = true) {
            velocity = direction.normalized * launchSpeed;
            if (align) {
                transform.forward = direction;
            }
            fired = true;
        }

        protected virtual void OnSpawned() {
            fired = false;
        }

        protected virtual bool OnHitCollider(Collider col, Vector3 position) {
            if (ShouldHitObject(col.transform, position) && (hitsTriggers || !col.isTrigger)) {
                OnHitObject(col.transform, position);
                return true;
            } else {
                return false;
            }
        }

        protected virtual bool ShouldHitObject(Transform col, Vector3 position) {
            return fired || hitsIfNotFired;
        }

        protected virtual void OnHitObject(Transform col, Vector3 position) {
            Vector3 impact = velocity * impactForce;
            col.Damage(new PointDamage(damage * damageMultiplier, position, velocity.normalized, velocity.magnitude * impactForce));

            if (impactForce > 0) {
                var rb = col.GetComponentInParent<Rigidbody>();
                if (rb) rb.AddForce(impact, ForceMode.Impulse);
                var rb2d = col.GetComponentInParent<Rigidbody2D>();
                if (rb2d) rb2d.AddForce(impact, ForceMode2D.Impulse);
            }

            HitObject?.Invoke(col.gameObject, position);

            if (stopOnHit) {
                velocity = Vector3.zero;
            }

            if (destroyOnHit) {
                Spawnable.Despawn(gameObject, linger);
                fired = false;
            }

            if (impactSound) {
                impactSound.PlayAtPoint(transform.position);
            }

            if (impactPrefab) {
                Spawnable.Spawn(impactPrefab, position, transform.rotation, parentImpactObject ? col : null, true);
            }
        }
    }
}