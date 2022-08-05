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
using Infohazard.Core;
using Infohazard.Sequencing;
using UnityEngine;

namespace SBR {
    /// <summary>
    /// Used to give a GameObject a health value.
    /// </summary>
    [DisallowMultipleComponent]
    public class Health : PersistedComponent<Health.StateInfo>, IParentDamageable {
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
        public float CurrentHealth {
            get => State?.Health ?? maxHealth;
            set {
                value = Mathf.Max(value, 0);
                if (State.Health == value) return;
                State.Health = value;
                if (State.Health == 0) {
                    SendDeathMessage();
                }
                State.NotifyStateChanged();
            }
        }

        /// <summary>
        /// Whether this Health is dead (has reached zero), and has not been revived.
        /// </summary>
        public bool Dead => CurrentHealth == 0;

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

        public float healthRegenThreshold = 0.3f;

        /// <summary>
        /// Whether to allow revival by healing after health has reached zero.
        /// </summary>
        [Tooltip("Whether to allow revival by healing after health has reached zero.")]
        public bool allowRevival = true;

        public bool persisted = true;

        /// <summary>
        /// Sound to play when damaged.
        /// </summary>
        [Tooltip("Sound to play when damaged")]
        public AudioParameters damageSound;

        /// <summary>
        /// Timer used for regeneration delay.
        /// </summary>
        public float TimeUntilRegen {
            get => State?.TimeUntilRegen ?? 0;
            set {
                value = Mathf.Max(value, 0);
                if (value == State.TimeUntilRegen) return;
                State.TimeUntilRegen = value;
                State.NotifyStateChanged();
            }
        }

        /// <summary>
        /// Timer used to check if invulnerable from hit.
        /// </summary>
        public float TimeUntilNotInvuln {
            get => State?.TimeUntilNotInvuln ?? 0;
            set {
                value = Math.Max(value, 0);
                if (value == State.TimeUntilNotInvuln) return;
                State.TimeUntilNotInvuln = value;
                State.NotifyStateChanged();
            }
        }
        
        public delegate void Modifier<T>(ref T input);

        protected virtual void Awake() {
            if (!persisted) CreateFakeState();
        }

        protected virtual void OnSpawned() {
            if (!persisted) CreateFakeState();
        }

        public override void LoadDefaultState() {
            TimeUntilRegen = 0;
            TimeUntilNotInvuln = 0;
            CurrentHealth = maxHealth;
        }

        public override void LoadState() {
            base.LoadState();
            if (!persisted) LoadDefaultState();
            if (Dead) SendDeathMessage();
        }

        protected virtual void Start() {
            Created?.Invoke(this);
        }

        protected virtual void Update() {
            float healthRatio = CurrentHealth / maxHealth;

            if (TimeUntilRegen > 0) TimeUntilRegen -= Time.deltaTime;
            if (TimeUntilNotInvuln > 0) TimeUntilNotInvuln -= Time.deltaTime;
            
            if (TimeUntilRegen == 0 && !Dead && healthRegenRate > 0 && healthRatio < healthRegenThreshold) {
                float healing = Mathf.Min(healthRegenThreshold * maxHealth - CurrentHealth, healthRegenRate * Time.deltaTime);
                Heal(healing);
            }
        }

        protected virtual void OnDestroy() {
            Destroyed?.Invoke(this);
        }

        private void SendDeathMessage() {
            SendMessage("OnZeroHealth", SendMessageOptions.DontRequireReceiver);
            ZeroHealth?.Invoke();
        }

        /// <summary>
        /// Apply damage to the Health.
        /// </summary>
        /// <param name="dmg">The damage to apply.</param>
        /// <returns>The actual damage amount dealt.</returns>
        public virtual float Damage(Damage dmg) {
            if (enabled && dmg.Amount > 0 && TimeUntilNotInvuln == 0) {
                DamageModifier?.Invoke(ref dmg);

                float prevHealth = CurrentHealth;
                CurrentHealth -= dmg.Amount;
                dmg.Amount = prevHealth - CurrentHealth;

                TimeUntilNotInvuln = hitInvuln;
                TimeUntilRegen = healthRegenDelay;
                SendMessage("OnDamage", dmg, SendMessageOptions.DontRequireReceiver);
                Damaged?.Invoke(dmg);
                if (damageSound) {
                    damageSound.Play();
                }

            } else {
                dmg.Amount = 0;
            }

            return dmg.Amount;
        }

        /// <summary>
        /// Apply healing to the Health. If health value has reached zero, will not work unless allowRevival is true.
        /// </summary>
        /// <param name="amount">The amount of healing.</param>
        /// <returns>The actual healing amount applied.</returns>
        public virtual float Heal(float amount) {
            if (enabled && (!Dead || allowRevival) && amount > 0) {
                HealingModifier?.Invoke(ref amount);

                float prevHealth = CurrentHealth;
                bool prevDead = Dead;
                CurrentHealth += amount;
                CurrentHealth = Mathf.Min(CurrentHealth, maxHealth);
                amount = CurrentHealth - prevHealth;
                SendMessage("OnHeal", amount, SendMessageOptions.DontRequireReceiver);
                Healed?.Invoke(amount);

                if (prevDead) {
                    SendMessage("OnRevive", SendMessageOptions.DontRequireReceiver);
                    Revived?.Invoke();
                }
            } else {
                amount = 0;
            }

            return amount;
        }
        
        [Serializable]
        public class StateInfo : PersistedData {
            public float TimeUntilRegen { get; set; }
            public float TimeUntilNotInvuln { get; set; }
            public float Health { get; set; }
        }
    }

    /// <summary>
    /// Contains extension methods for applying damage without checking for a Health component.
    /// </summary>
    public static class HealthExt {
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

        /// <summary>
        /// Try to get the current health of a GameObject or it's ancestors.
        /// </summary>
        /// <param name="obj">The object to test.</param>
        /// <param name="health">Health value in object's (or parent's) Health component, or zero.</param>
        /// <returns>Whether there was a Health component.</returns>
        public static bool TryGetHealth(this GameObject obj, out float health) {
            if (obj.TryGetComponentInParent<Health>(out var h)) {
                health = h.CurrentHealth;
                return true;
            } else {
                health = 0;
                return false;
            }
        }

        /// <summary>
        /// Get the current health of a GameObject or it's ancestors. Throw an exception if not found.
        /// </summary>
        /// <param name="obj">The object to test.</param>
        /// <returns>Health value in object's (or parent's) Health component.</returns>
        /// <exception cref="MissingComponentException">If there is no Health component.</exception>
        public static float GetHealth(this GameObject obj) {
            if (TryGetHealth(obj, out var health)) {
                return health;
            } else {
                throw new MissingComponentException($"{obj} has no Health component.");
            }
        }

        /// <summary>
        /// Returns true if the object has a health value > 0, or has no Health component.
        /// </summary>
        /// <param name="obj">The object to test.</param>
        /// <returns>True if the object has a health component with a health value > 0, or has no health component. False otherwise.</returns>
        public static bool IsAlive(this GameObject obj) {
            if (TryGetHealth(obj, out var health)) {
                return health > 0;
            } else {
                return true;
            }
        }
    }
}
