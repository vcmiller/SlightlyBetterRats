using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SBR {
    /// <summary>
    /// Used to give a GameObject a health value.
    /// </summary>
    [DisallowMultipleComponent]
    public class Health : MonoBehaviour {
        /// <summary>
        /// Invoked when a new Health is created.
        /// </summary>
        public static event Action<Health> Created;

        /// <summary>
        /// Invoked when a Health is destroyed.
        /// </summary>
        public static event Action<Health> Destroyed;

        /// <summary>
        /// Invoked when this Health is damaged.
        /// </summary>
        public event Action<Damage> Damaged;

        /// <summary>
        /// Invoked when this Health reaches zero.
        /// </summary>
        public event Action ZeroHealth;

        /// <summary>
        /// Invoked when this Health is healed.
        /// </summary>
        public event Action<float> Healed;

        /// <summary>
        /// Invoked when this Health is revived (healed from zero).
        /// </summary>
        public event Action Revived;

        /// <summary>
        /// Invoked before this Health is damaged, in order to modify damage value.
        /// For example, this can be used to create damage absorbtion.
        /// </summary>
        public event Modifier<Damage> DamageModifier;

        /// <summary>
        /// Invoked before this Health is healed, in order to modify healing value.
        /// </summary>
        public event Modifier<float> HealingModifier;

        /// <summary>
        /// The current health value.
        /// </summary>
        public float health { get; private set; }

        /// <summary>
        /// Whether this Health is dead (has reached zero), and has not been revived.
        /// </summary>
        public bool dead { get; private set; }

        /// <summary>
        /// Maximum health value.
        /// </summary>
        [Tooltip("Maximum health value.")]
        public float maxHealth = 100;

        /// <summary>
        /// On hit, become invulnerable for this amount of time.
        /// </summary>
        [Tooltip("On hit, become invulnerable for this amount of time.")]
        public float hitInvuln = 0;

        /// <summary>
        /// Rate at which health regenerates (zero for no regen).
        /// </summary>
        [Tooltip("Rate at which health regenerates (zero for no regen).")]
        public float healthRegenRate = 0;

        /// <summary>
        /// Delay after taking damage before health starts regenerating.
        /// </summary>
        [Tooltip("Delay after taking damage before health starts regenerating.")]
        public float healthRegenDelay = 0;

        /// <summary>
        /// Whether to allow revival by healing after health has reached zero.
        /// </summary>
        [Tooltip("Whether to allow revival by healing after health has reached zero.")]
        public bool allowRevival = true;
        
        /// <summary>
        /// Timer used for regeneration delay.
        /// </summary>
        public ExpirationTimer healthRegenTimer { get; private set; }

        /// <summary>
        /// Timer used to check if invulnerable from hit.
        /// </summary>
        public CooldownTimer hitInvulnTimer { get; private set; }
        
        public delegate void Modifier<T>(ref T input);

        protected virtual void Awake() {
            health = maxHealth;

            hitInvulnTimer = new CooldownTimer(hitInvuln, 0);
            healthRegenTimer = new ExpirationTimer(healthRegenDelay);
        }

        protected virtual void Start() {
            Created?.Invoke(this);
        }

        protected virtual void Update() {
            if (healthRegenTimer.expired && !dead && healthRegenRate > 0) {
                Heal(healthRegenRate * Time.deltaTime);
            }
        }

        protected virtual void OnDestroy() {
            Destroyed?.Invoke(this);
        }

        /// <summary>
        /// Apply damage to the Health.
        /// </summary>
        /// <param name="amount">The amount of damage.</param>
        /// <returns>The actual damage amount dealt.</returns>
        public float Damage(float amount) {
            return Damage(new Damage(amount));
        }

        /// <summary>
        /// Apply damage to the Health.
        /// </summary>
        /// <param name="dmg">The damage to apply.</param>
        /// <returns>The actual damage amount dealt.</returns>
        public virtual float Damage(Damage dmg) {
            if (enabled && hitInvulnTimer.Use() && dmg.amount > 0) {
                DamageModifier?.Invoke(ref dmg);

                float prevHealth = health;
                health -= dmg.amount;
                health = Mathf.Max(health, 0);
                dmg.amount = prevHealth - health;

                healthRegenTimer.Set();
                SendMessage("OnDamage", dmg, SendMessageOptions.DontRequireReceiver);
                Damaged?.Invoke(dmg);

                if (health == 0 && !dead) {
                    dead = true;
                    SendMessage("OnZeroHealth", SendMessageOptions.DontRequireReceiver);
                    ZeroHealth?.Invoke();
                }
            } else {
                dmg.amount = 0;
            }

            return dmg.amount;
        }

        /// <summary>
        /// Apply healing to the Health. If health value has reached zero, will not work unless allowRevival is true.
        /// </summary>
        /// <param name="amount">The amount of healing.</param>
        /// <returns>The actual healing amount applied.</returns>
        public virtual float Heal(float amount) {
            if (enabled && (!dead || allowRevival) && amount > 0) {
                HealingModifier?.Invoke(ref amount);

                float prevHealth = health;
                health += amount;
                health = Mathf.Min(health, maxHealth);
                amount = health - prevHealth;
                SendMessage("OnHeal", amount, SendMessageOptions.DontRequireReceiver);
                Healed?.Invoke(amount);

                if (dead) {
                    dead = false;
                    SendMessage("OnRevive", SendMessageOptions.DontRequireReceiver);
                    Revived?.Invoke();
                }
            } else {
                amount = 0;
            }

            return amount;
        }
    }

    /// <summary>
    /// Contains extension methods for applying damage without checking for a Health component.
    /// </summary>
    public static class HealthExt {
        /// <summary>
        /// Damage the GameObject if it has a Health component.
        /// </summary>
        /// <param name="obj">The GameObject to damage.</param>
        /// <param name="dmg">The damage object.</param>
        /// <returns>The actual damage amount dealt.</returns>
        public static float Damage(this GameObject obj, Damage dmg) {
            Health h = obj.GetComponentInParent<Health>();
            if (h) {
                return h.Damage(dmg);
            } else {
                return 0;
            }
        }

        /// <summary>
        /// Damage the GameObject if it has a Health component.
        /// </summary>
        /// <param name="obj">The GameObject to damage.</param>
        /// <param name="amount">The amount of damage.</param>
        /// <returns>The actual damage amount dealt.</returns>
        public static float Damage(this GameObject obj, float amount) {
            return obj.Damage(new Damage(amount));
        }

        /// <summary>
        /// Damage the GameObject if it has a Health component.
        /// </summary>
        /// <param name="cmp">The Component of the GameObject to damage.</param>
        /// <param name="dmg">The damage object.</param>
        /// <returns>The actual damage amount dealt.</returns>
        public static float Damage(this Component cmp, Damage dmg) {
            return cmp.gameObject.Damage(dmg);
        }

        /// <summary>
        /// Damage the GameObject if it has a Health component.
        /// </summary>
        /// <param name="obj">The Component of the GameObject to damage.</param>
        /// <param name="amount">The amount of damage.</param>
        /// <returns>The actual damage amount dealt.</returns>
        public static float Damage(this Component cmp, float amount) {
            return cmp.gameObject.Damage(amount);
        }

        /// <summary>
        /// Heal the GameObject if it has a Health component,
        /// and revive it if health is zero and allowRevive is true.
        /// </summary>
        /// <param name="obj">The GameObject to heal.</param>
        /// <param name="amount">The amount of healing to do.</param>
        /// <returns>The actual healing done.</returns>
        public static float Heal(this GameObject obj, float amount) {
            Health h = obj.GetComponentInParent<Health>();
            if (h) {
                return h.Heal(amount);
            } else {
                return 0;
            }
        }

        /// <summary>
        /// Heal the GameObject if it has a Health component,
        /// and revive it if health is zero and allowRevive is true.
        /// </summary>
        /// <param name="cmp">The Component of the GameObject to heal.</param>
        /// <param name="amount">The amount of healing to do.</param>
        /// <returns>The actual healing done.</returns>
        public static float Heal(this Component cmp, float amount) {
            return cmp.gameObject.Heal(amount);
        }
    }
}
