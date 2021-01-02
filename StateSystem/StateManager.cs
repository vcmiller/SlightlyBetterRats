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

namespace SBR.StateSystem {
    public class StateManager : MonoBehaviour {
        [SerializeField] private StateManager _parent;
        
        private IStateBehaviour[] _behaviours;
        private Dictionary<string, bool> _statesActive;
        private List<StateManager> _children;

        public StateManager Parent {
            get => _parent;
            set {
                if (value != Parent) {
                    if (_parent) _parent._children.Remove(this);
                    if (value) value._children.Add(this);

                    _parent = value;
                    ApplyInternalStates();
                }
            }
        }

        private void Awake() {
            _statesActive = new Dictionary<string, bool>();
            _children = new List<StateManager>();
            RefreshStateBehaviours();
        }

        private void Start() {
            if (!_parent) return;
            
            StateManager tempParent = _parent;
            _parent = null;
            Parent = tempParent;
        }

        private void OnDestroy() {
            if (Parent) {
                Parent._children.Remove(this);
            }

            foreach (StateManager child in _children) {
                child.Parent = null;
            }
        }

        private void ApplyInternalStates() {
            var activeDict = Parent ? Parent._statesActive : _statesActive;
            if (activeDict == null) return;

            foreach (var item in activeDict) {
                SetStateActiveInternal(item.Key, item.Value);
            }
        }

        public void RefreshStateBehaviours() {
            _behaviours = GetComponentsInChildren<IStateBehaviour>(true);
            ApplyInternalStates();
        }

        public bool IsStateActiveOnAny(string state) => _behaviours.Any(b => b.IsStateActive(state));
        public bool IsStateActiveOnAll(string state) => _behaviours.All(b => b.IsStateActive(state));
        public void ForgetState(string state) => _statesActive.Remove(state);
        public void SetStateActive(string state, bool active) {
            if (_statesActive.TryGetValue(state, out bool curValue) && curValue == active) return;

            _statesActive[state] = active;

            if (!_parent) {
                SetStateActiveInternal(state, active);
            }
        }

        private void SetStateActiveInternal(string state, bool active) {
            foreach (IStateBehaviour b in _behaviours) {
                b.SetStateActive(state, active);
            }

            foreach (StateManager child in _children) {
                child.SetStateActiveInternal(state, active);
            }
        }
    }
}