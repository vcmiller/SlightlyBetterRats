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

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

using Object = UnityEngine.Object;

namespace SBR.StateSystem {
    [Serializable]
    public struct State {
        [FormerlySerializedAs("name")] [SerializeField]
        private string _name;

        [FormerlySerializedAs("values")] [SerializeField]
        private ComponentOverride _values;

        [FormerlySerializedAs("blockedBy")] [SerializeField]
        private int _blockedBy;

        [FormerlySerializedAs("isActive")] [SerializeField]
        private bool _isActive;

        public bool IsBlocked { get; set; }
        public int[] FieldIDs { get; set; }
        public string Name => _name;
        public ComponentOverride Values => _values;
        public int BlockedBy => _blockedBy;
        public bool IsActive {
            get => _isActive;
            set => _isActive = value;
        }
    }

    [Serializable]
    public class StateList {
        [FormerlySerializedAs("states")] [SerializeField]
        private State[] _states;
        
        private Object _owner;
        private object[] _fieldDefaultValues;
        public StateList() { }
        public StateList(State[] states) {
            _states = states;
        }

        public void Initialize(Object owner) {
            _owner = owner;

            var fieldIDs = new Dictionary<string, int>();
            List<bool> statesActive = _states.Select(s => s.IsActive).ToList();
            for (int i = 0; i < _states.Length; i++) {
                ComponentOverride values = _states[i].Values;

                _states[i].FieldIDs = new int[values ? values.Overrides.Length : 0];
                _states[i].IsActive = false;
                _states[i].IsBlocked = false;

                for (int j = 0; j < _states[i].FieldIDs.Length; j++) {
                    if (!fieldIDs.TryGetValue(values.Overrides[j].Path, out int fieldID)) {
                        fieldID = fieldIDs.Count;
                        fieldIDs[values.Overrides[j].Path] = fieldID;
                    }

                    _states[i].FieldIDs[j] = fieldID;
                }
            }

            _fieldDefaultValues = new object[fieldIDs.Count];

            for (int i = 0; i < _states.Length; i++) {
                if (statesActive[i]) {
                    SetStateActive(i, true);
                }
            }
        }

        private int GetStateIndex(string name) {
            for (int i = 0; i < _states.Length; i++) {
                if (_states[i].Name == name) {
                    return i;
                }
            }
            return -1;
        }

        public bool HasState(string state) {
            return GetStateIndex(state) >= 0;
        }

        private bool IsStateBlocked(int stateIndex) {
            if (_states[stateIndex].BlockedBy == 0) return false;

            for (int i = 0; i < _states.Length; i++) {
                if (IsStateBlockedBy(stateIndex, i)) {
                    return true;
                }
            }

            return false;
        }

        private bool IsStateBlockedBy(int state, int blockingState) {
            return state != blockingState && 
                _states[blockingState].IsActive && 
                !_states[blockingState].IsBlocked && 
                (_states[state].BlockedBy & (1 << blockingState)) != 0;
        }

        public bool IsStateActive(string state) {
            int index = GetStateIndex(state);
            if (index < 0) return false;
            return IsStateActive(index);
        }

        public bool IsStateActive(int stateIndex) {
            return _states[stateIndex].IsActive;
        }

        public void SetStateActive(string state, bool active) {
            int stateIndex = GetStateIndex(state);
            if (stateIndex >= 0) {
                SetStateActive(stateIndex, active);
            }
        }

        public void SetStateActive(int stateIndex, bool active) {
            if (_states[stateIndex].IsActive == active) return;

            UpdateActiveAndBlocked(stateIndex, active, IsStateBlocked(stateIndex));
        }

        private void UpdateActiveAndBlocked(int stateIndex, bool active, bool isBlocked) {
            bool fieldStatusChanging = (active && !isBlocked) != (_states[stateIndex].IsActive && !_states[stateIndex].IsBlocked);

            _states[stateIndex].IsBlocked = isBlocked;
            _states[stateIndex].IsActive = active;

            if (fieldStatusChanging) {
                bool isActivating = active && !isBlocked;
                for (int i = 0; i < _states[stateIndex].FieldIDs.Length; i++) {
                    if (isActivating) {
                        ActivateField(stateIndex, i);
                    } else {
                        DeactivateField(stateIndex, i);
                    }
                }
            }

            for (int i = 0; i < _states.Length; i++) {
                if (i != stateIndex && 
                    _states[i].IsActive && 
                    IsStateBlockedBy(i, stateIndex) != _states[i].IsBlocked &&
                    IsStateBlocked(i) != _states[i].IsBlocked) {
                    UpdateActiveAndBlocked(i, _states[i].IsActive, !_states[i].IsBlocked);
                }
            }
        }

        private int GetFirstStateControllingField(int fieldID, int excludeState, out SerializedValueOverride value) {
            for (int i = 0; i < _states.Length; i++) {
                if (i == excludeState || !_states[i].IsActive || _states[i].IsBlocked) continue;

                int index = Array.IndexOf(_states[i].FieldIDs, fieldID);
                if (index >= 0) {
                    value = _states[i].Values.Overrides[index];
                    return i;
                }
            }

            value = null;
            return -1;
        }

        private void ActivateField(int stateIndex, int localFieldIndex) {
            int fieldID = _states[stateIndex].FieldIDs[localFieldIndex];
            int previous = GetFirstStateControllingField(fieldID, stateIndex, out _);

            if (previous >= 0 && previous < stateIndex) return;

            var valueOverride = _states[stateIndex].Values.Overrides[localFieldIndex];
            if (previous < 0) {
                _fieldDefaultValues[fieldID] = valueOverride.GetFieldValue(_owner);
            }
            valueOverride.SetFieldValue(_owner);
        }

        private void DeactivateField(int stateIndex, int localFieldIndex) {
            int fieldID = _states[stateIndex].FieldIDs[localFieldIndex];
            int previous = GetFirstStateControllingField(fieldID, stateIndex, out var previousValueOverride);

            if (previous >= 0 && previous < stateIndex) return;

            var valueOverride = _states[stateIndex].Values.Overrides[localFieldIndex];
            if (previous < 0) {
                valueOverride.SetFieldValue(_owner, _fieldDefaultValues[fieldID]);
            } else {
                previousValueOverride.SetFieldValue(_owner);
            }
        }
    }
}