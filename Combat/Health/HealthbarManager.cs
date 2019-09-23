using System.Collections.Generic;
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
        [MultiEnum]
        public Tag shownTags;

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
            return healthbarPrefab && health.CompareTag(shownTags) && !healthbars.ContainsKey(health);
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
