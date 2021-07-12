using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SBR.Persistence {
    public class PersistedComponentEnabled : PersistedComponent<PersistedComponentEnabled.StateInfo> {
        [SerializeField] private Behaviour _target = null;
        
        public override void LoadState() {
            base.LoadState();
            if (_target) _target.enabled = State.Enabled;
        }

        public override void WillSaveState() {
            base.WillSaveState();
            if (_target) State.Enabled = _target.enabled;
        }

        public class StateInfo : PersistedData {
            private bool _enabled;

            public bool Enabled {
                get => _enabled;
                set {
                    if (_enabled == value) return;
                    _enabled = value;
                    NotifyStateChanged();
                }
            }
        }
    }
}