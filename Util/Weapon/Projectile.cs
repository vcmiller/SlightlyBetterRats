﻿using System.Collections;
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
        /// Prefab to spawn on impact, such as an explosion.
        /// </summary>
        [Tooltip("Prefab to spawn on impact, such as an explosion.")]
        public GameObject impactPrefab;

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

        protected virtual void OnHitCollider(Collider col, Vector3 position) {
            if (hitsTriggers || !col.isTrigger) {
                OnHitObject(col.transform, position);
            }
        }

        protected virtual void OnHitObject(Transform col, Vector3 position) {
            if (!fired && !hitsIfNotFired) {
                return;
            }

            Vector3 impact = velocity * impactForce;
            col.Damage(new PointDamage(damage, position, impact));

            if (impactForce > 0) {
                var rb = col.GetComponentInParent<Rigidbody>();
                if (rb) rb.AddForce(impact, ForceMode.Impulse);
                var rb2d = col.GetComponentInParent<Rigidbody2D>();
                if (rb2d) rb2d.AddForce(impact, ForceMode2D.Impulse);
            }

            velocity = Vector3.zero;
            transform.position = position;

            if (destroyOnHit) {
                Destroy(gameObject, linger);
            }

            if (impactSound) {
                impactSound.PlayAtPoint(transform.position);
            }

            if (impactPrefab) {
                Instantiate(impactPrefab, position, transform.rotation);
            }
        }
    }
}