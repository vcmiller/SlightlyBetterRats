using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SBR {
    /// <summary>
    /// Destroys a GameObject after a set amount of time.
    /// The field timeToLive can be modified in code to extend or reduce the lifetime.
    /// </summary>
    public class TimeToLive : MonoBehaviour {
        /// <summary>
        /// How much time remains before the GameObject is destroyed.
        /// </summary>
        [Tooltip("How much time remains before the GameObject is destroyed.")]
        public float timeToLive;

        private void Update() {
            timeToLive -= Time.deltaTime;

            if (timeToLive <= 0) {
                Destroy(gameObject);
            }
        }
    }
}