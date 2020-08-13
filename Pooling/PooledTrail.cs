using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SBR {
    [RequireComponent(typeof(TrailRenderer))]
    public class PooledTrail : MonoBehaviour {
        private TrailRenderer _trail = null;

        private void Awake() {
            _trail = GetComponent<TrailRenderer>();
        }

        private void OnSpawned() {
            _trail.Clear();
        }
    }
}