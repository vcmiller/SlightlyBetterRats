
using UnityEngine;

namespace SBR {
    /// <summary>
    /// Used to create an expiration, or condition for a period of time after an action occurs.
    /// </summary>
    public class ExpirationTimer {
        /// <summary>
        /// How long after being set the timer expires.
        /// </summary>
        public float expiration { get; set; }

        /// <summary>
        /// Time at which the timer was last used. Comparable to curTime.
        /// </summary>
        public float lastSet { get; set; }

        /// <summary>
        /// Whether to use unscaled time (so it works when the game is paused).
        /// </summary>
        public bool unscaled { get; set; }

        /// <summary>
        /// Returns Time.time if scaled, Time.unscaledTime if unscaled.
        /// </summary>
        public float curTime => unscaled ? Time.unscaledTime : Time.time;

        private bool set;

        /// <summary>
        /// Returns Time.deltaTime if scaled, Time.unscaledDeltaTie if unscald.
        /// </summary>
        private float deltaTime => unscaled ? Time.unscaledDeltaTime : Time.deltaTime;

        /// <summary>
        /// Whether the timer has expired.
        /// </summary>
        public bool expired => !set || curTime >= lastSet + expiration;

        /// <summary>
        /// Whether the timer expired during the current frame.
        /// </summary>
        public bool expiredThisFrame => set && expired && curTime - deltaTime <= lastSet + expiration;
        
        /// <summary>
        /// How much time remains before the timer expires.
        /// </summary>
        public float remaining => Mathf.Max(0, expiration - (curTime - lastSet));

        /// <summary>
        /// How far the timer is from expiring in the range [0, 1].
        /// If exired, is 0. If just set, is 1.
        /// </summary>
        public float remainingRatio {
            get {
                if (expired) {
                    return 0;
                } else {
                    return 1 - ((curTime - lastSet) / expiration);
                }
            }
        }

        /// <summary>
        /// Create a new ExpirationTimer with given expiration. Expired will be true initially.
        /// </summary>
        /// <param name="expiration">Expiration value.</param>
        public ExpirationTimer(float expiration) {
            this.expiration = expiration;
            Clear();
        }

        /// <summary>
        /// Set the timer so that expired will be false for expiration seconds.
        /// </summary>
        public void Set() {
            lastSet = curTime;
            set = true;
        }

        /// <summary>
        /// Clear the timer so that expired will be true until it is set again.
        /// </summary>
        public void Clear() {
            lastSet = curTime - expiration;
            set = false;
        }
    }
}