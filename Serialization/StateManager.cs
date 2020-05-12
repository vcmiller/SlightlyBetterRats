// MIT License
// 
// Copyright (c) 2020 Vincent Miller
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