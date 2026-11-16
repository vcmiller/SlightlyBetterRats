// The MIT License (MIT)
// 
// Copyright (c) 2026-present Val Miller
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

using UnityEngine;

namespace SBR {
    /// <summary>
    /// Used to create a cooldown, or action for which a delay must pass before it can happen again.
    /// </summary>
    public class CooldownTimer {
        /// <summary>
        /// Delay that must pass between uses.
        /// </summary>
        public float Cooldown { get; set; }

        /// <summary>
        /// Time at which the timer was last used. Comparable to curTime.
        /// </summary>
        public float LastUse { get; set; }

        /// <summary>
        /// Whether to use unscaled time (so it works when the game is paused).
        /// </summary>
        public bool Unscaled { get; set; }

        /// <summary>
        /// Returns Time.time if scaled, Time.unscaledTime if unscaled.
        /// </summary>
        public float CurTime => Unscaled ? Time.unscaledTime : Time.time;

        /// <summary>
        /// How close the timer is to being ready to use. Has value 1.0 if ready, 0.0 if just used.
        /// </summary>
        public float ChargeRatio {
            get {
                if (CanUse) {
                    return 1.0f;
                } else {
                    return (CurTime - LastUse) / Cooldown;
                }
            }
        }

        public float TimeUntilUsable {
            get {
                if (CanUse) {
                    return 0.0f;
                } else {
                    return Cooldown - (CurTime - LastUse);
                }
            }
        }

        /// <summary>
        /// Whether the timer can currently be used.
        /// </summary>
        public bool CanUse => CurTime - LastUse >= Cooldown;

        /// <summary>
        /// Create a new CooldownTimer with given cooldown.
        /// The cooldown will have to pass before it can be used for the first time.
        /// </summary>
        /// <param name="cooldown">The cooldown of the timer.</param>
        public CooldownTimer(float cooldown) {
            this.Cooldown = cooldown;
            this.LastUse = CurTime;
        }

        /// <summary>
        /// Create a new CooldownTimer with given coooldown, and a different initial cooldown.
        /// </summary>
        /// <param name="cooldown">The cooldown of the timer.</param>
        /// <param name="initial">The time that must pass before the first use.</param>
        public CooldownTimer(float cooldown, float initial) {
            this.Cooldown = cooldown;
            this.LastUse = CurTime - cooldown + initial;
        }

        /// <summary>
        /// Try to use the timer. 
        /// If the cooldown has passed since the last use, reset last use and return true.
        /// Otherwise, return false and do not change last use.
        /// </summary>
        /// <returns>Whether the timer was used.</returns>
        public bool Use() {
            if (CanUse) {
                Reset();
                return true;
            } else {
                return false;
            }
        }

        /// <summary>
        /// Set the timer to be ready to use.
        /// </summary>
        public void Clear() => LastUse = CurTime - Cooldown;

        /// <summary>
        /// Reset the timer to require the cooldown before it can be used.
        /// </summary>
        public void Reset() => LastUse = CurTime;
    }
}

