
using System;
using UnityEngine;

namespace SBR {
    /// <summary>
    /// Used to create a cooldown, or action for which a delay must pass before it can happen again.
    /// </summary>
    public class CooldownTimer {
        /// <summary>
        /// Delay that must pass between uses.
        /// </summary>
        public float cooldown { get; set; }

        /// <summary>
        /// Time at which the timer was last used. Comparable to curTime.
        /// </summary>
        public float lastUse { get; set; }

        /// <summary>
        /// Whether to use unscaled time (so it works when the game is paused).
        /// </summary>
        public bool unscaled { get; private set; }

        /// <summary>
        /// Returns Time.time if scaled, Time.unscaledTime if unscaled.
        /// </summary>
        public float curTime => unscaled ? Time.unscaledTime : Time.time;

        /// <summary>
        /// How close the timer is to being ready to use. Has value 1.0 if ready, 0.0 if just used.
        /// </summary>
        public float chargeRatio {
            get {
                if (canUse) {
                    return 1.0f;
                } else {
                    return (curTime - lastUse) / cooldown;
                }
            }
        }

        public float timeUntilUsable {
            get {
                if (canUse) {
                    return 0.0f;
                } else {
                    return cooldown - (curTime - lastUse);
                }
            }
        }

        /// <summary>
        /// Whether the timer can currently be used.
        /// </summary>
        public bool canUse => curTime - lastUse >= cooldown;

        /// <summary>
        /// Create a new CooldownTimer with given cooldown.
        /// The cooldown will have to pass before it can be used for the first time.
        /// </summary>
        /// <param name="cooldown">The cooldown of the timer.</param>
        public CooldownTimer(float cooldown) {
            this.cooldown = cooldown;
            this.lastUse = curTime;
        }

        /// <summary>
        /// Create a new CooldownTimer with given coooldown, and a different initial cooldown.
        /// </summary>
        /// <param name="cooldown">The cooldown of the timer.</param>
        /// <param name="initial">The time that must pass before the first use.</param>
        public CooldownTimer(float cooldown, float initial) {
            this.cooldown = cooldown;
            this.lastUse = curTime - cooldown + initial;
        }

        /// <summary>
        /// Try to use the timer. 
        /// If the cooldown has passed since the last use, reset last use and return true.
        /// Otherwise, return false and do not change last use.
        /// </summary>
        /// <returns>Whether the timer was used.</returns>
        public bool Use() {
            if (canUse) {
                Reset();
                return true;
            } else {
                return false;
            }
        }

        /// <summary>
        /// Set the timer to be ready to use.
        /// </summary>
        public void Clear() => lastUse = curTime - cooldown;

        /// <summary>
        /// Reset the timer to require the cooldown before it can be used.
        /// </summary>
        public void Reset() => lastUse = curTime;
    }
}

