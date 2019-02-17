using UnityEngine;
using System.Collections;

namespace SBR {
    /// <summary>
    /// Used to create an action that can only happen a set number of times before it must recharge,
    /// such as bullets in a magazine that must be reloaded.
    /// </summary>
    public class Magazine {
        private ExpirationTimer reload;

        /// <summary>
        /// How many uses remain before a reload is necessary.
        /// </summary>
        public int remainingShots { get; private set; }

        /// <summary>
        /// How many times it can be used per reload.
        /// </summary>
        public int clipSize { get; private set; }

        /// <summary>
        /// Whether it can currently be used.
        /// </summary>
        public bool canFire => remainingShots > 0 && reload.expired;

        /// <summary>
        /// Whether is currently reloading.
        /// </summary>
        public bool reloading => !reload.expired;

        /// <summary>
        /// Construct a new Magazine with given size and reload time.
        /// </summary>
        /// <param name="size">Number of uses before reload.</param>
        /// <param name="reloadTime">Time it takes to reload.</param>
        public Magazine(int size, float reloadTime) {
            reload = new ExpirationTimer(reloadTime);
            remainingShots = size;
            clipSize = size;
        }

        /// <summary>
        /// Manually reload.
        /// </summary>
        public void Reload() {
            if (remainingShots < clipSize) {
                remainingShots = clipSize;
                reload.Set();
            }
        }

        /// <summary>
        /// Use the Magazine once if possible, and return true.
        /// Otherwise, return false.
        /// If remaining uses reaches 0, reload.
        /// </summary>
        /// <returns>Whether it could be used.</returns>
        public bool Fire() {
            if (canFire) {
                remainingShots--;

                if (remainingShots == 0) {
                    Reload();
                }

                return true;
            } else {
                return false;
            }
        }
    }
}