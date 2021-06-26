using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SBR.Persistence {
    [RequireComponent(typeof(TriggerVolume))]
    public class RegionTransitionTrigger : MonoBehaviour {
        [SerializeField] private PersistedRegionRoot _transitionTo;
        
        private TriggerVolume _volume;
        
        private void Awake() {
            _volume = GetComponent<TriggerVolume>();
            _volume.TriggerEntered += Volume_TriggerEntered;
        }

        private void Volume_TriggerEntered(GameObject obj) {
            if (!_transitionTo || !_transitionTo.Initialized) return;
            if (obj.TryGetComponentInParent(out PersistedGameObject pgo) && pgo.CanTransitionRegions &&
                pgo.Level == null && pgo.Region != _transitionTo && pgo.DynamicPrefabID != 0) {
                if (pgo.Initialized) {
                    pgo.TransitionToRegion(_transitionTo);
                }
                SceneManager.MoveGameObjectToScene(pgo.gameObject, _transitionTo.gameObject.scene);
            }
        }
    }
}