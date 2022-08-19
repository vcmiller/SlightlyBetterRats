// The MIT License (MIT)
// 
// Copyright (c) 2022-present Vincent Miller
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

using System.Collections.Generic;
using Infohazard.Core;
using UnityEngine;

namespace SBR {
    /// <summary>
    /// Used to automatically create Healthbars for all Health components that match a given tag.
    /// </summary>
    public class HealthbarManager : MonoBehaviour {
        private Dictionary<Health, Healthbar> healthbars = new Dictionary<Health, Healthbar>();

        /// <summary>
        /// Healthbar prefab to spawn for each Health.
        /// </summary>
        [Tooltip("Healthbar prefab to spawn for each Health.")]
        public Healthbar healthbarPrefab;

        /// <summary>
        /// Tags which this HealthbarManager applies to.
        /// </summary>
        [Tooltip("Tags which this HealthbarManager applies to.")]
        public TagMask shownTags;

        private void Awake() {
            Health.Created += HealthCreated;
            Health.Destroyed += HealthDestroyed;
        }

        private void Start() {
            foreach (var health in FindObjectsOfType<Health>()) {
                HealthCreated(health);
            }
        }

        private void OnDestroy() {
            Health.Created -= HealthCreated;
            Health.Destroyed -= HealthDestroyed;

            foreach (var hb in healthbars) {
                if (hb.Value) {
                    Destroy(hb.Value.gameObject);
                }
            }
        }

        private void OnEnable() {
            foreach (var hb in healthbars) {
                if (hb.Value) {
                    hb.Value.gameObject.SetActive(true);
                }
            }
        }

        private void OnDisable() {
            foreach (var hb in healthbars) {
                if (hb.Value) {
                    hb.Value.gameObject.SetActive(false);
                }
            }
        }

        private bool ShouldCreate(Health health) {
            return healthbarPrefab && health.gameObject.CompareTagMask(shownTags) && !healthbars.ContainsKey(health);
        }

        private void HealthCreated(Health health) {
            if (ShouldCreate(health)) {
                var hb = Instantiate(healthbarPrefab, transform, false);
                healthbars[health] = hb;
                hb.target = health;
                hb.gameObject.SetActive(enabled);
            }
        }

        private void HealthDestroyed(Health health) {
            if (healthbars.ContainsKey(health) && healthbars[health]) {
                Destroy(healthbars[health].gameObject);
                healthbars[health] = null;
            }
        }
    }
}
