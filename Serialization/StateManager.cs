using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SBR.Serialization {
    public class StateManager : MonoBehaviour {
        private IStateBehaviour[] behaviours;
        private Dictionary<string, bool> statesActive;

        [SerializeField] private StateManager _parent;
        private List<StateManager> children;

        public StateManager parent {
            get => _parent;
            set {
                if (value != parent) {
                    if (_parent) _parent.children.Remove(this);
                    if (value) value.children.Add(this);

                    _parent = value;
                    ApplyInternalStates();
                }
            }
        }

        private void Awake() {
            statesActive = new Dictionary<string, bool>();
            children = new List<StateManager>();
            RefreshStateBehaviours();
        }

        private void Start() {
            if (_parent) {
                var tempParent = _parent;
                _parent = null;
                parent = tempParent;
            }
        }

        private void OnDestroy() {
            if (parent) {
                parent.children.Remove(this);
            }

            foreach (var child in children) {
                child.parent = null;
            }
        }

        private void ApplyInternalStates() {
            var activeDict = parent ? parent.statesActive : statesActive;
            if (activeDict == null) return;

            foreach (var item in activeDict) {
                SetStateActiveInternal(item.Key, item.Value);
            }
        }

        public void RefreshStateBehaviours() {
            behaviours = GetComponentsInChildren<IStateBehaviour>();
            ApplyInternalStates();
        }

        public bool IsStateActiveOnAny(string state) => behaviours.Any(b => b.IsStateActive(state));
        public bool IsStateActiveOnAll(string state) => behaviours.All(b => b.IsStateActive(state));
        public void ForgetState(string state) => statesActive.Remove(state);
        public void SetStateActive(string state, bool active) {
            if (statesActive.ContainsKey(state) && statesActive[state] == active) return;

            statesActive[state] = active;

            if (!_parent) {
                SetStateActiveInternal(state, active);
            }
        }

        private void SetStateActiveInternal(string state, bool active) {
            foreach (var b in behaviours) {
                b.SetStateActive(state, active);
            }

            foreach (var child in children) {
                child.SetStateActiveInternal(state, active);
            }
        }
    }
}