﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SBR.Serialization {
    [Serializable]
    public struct State {
        public string name;
        public ComponentOverride values;
        public int blockedBy;
        public bool isActive;

        [NonSerialized] public bool isBlocked;
        [NonSerialized] public int[] fieldIDs;
    }

    [Serializable]
    public class StateList {

        [SerializeField] private State[] states;
        private Object owner;
        private object[] fieldDefaultValues;
        public StateList() { }
        public StateList(State[] states) {
            this.states = states;
        }

        public void Initialize(Object owner) {
            this.owner = owner;

            Dictionary<string, int> fieldIDs = new Dictionary<string, int>();
            bool[] statesActive = states.Select(s => s.isActive).ToArray();
            for (int i = 0; i < states.Length; i++) {
                var values = states[i].values;

                states[i].fieldIDs = new int[values.overrides.Length];
                states[i].isActive = false;
                states[i].isBlocked = false;

                for (int j = 0; j < values.overrides.Length; j++) {
                    if (!fieldIDs.TryGetValue(values.overrides[j].path, out int fieldID)) {
                        fieldID = fieldIDs.Count;
                        fieldIDs[values.overrides[j].path] = fieldID;
                    }

                    states[i].fieldIDs[j] = fieldID;
                }
            }

            fieldDefaultValues = new object[fieldIDs.Count];

            for (int i = 0; i < states.Length; i++) {
                if (statesActive[i]) {
                    SetStateActive(i, true);
                }
            }
        }

        private int GetStateIndex(string name) {
            for (int i = 0; i < states.Length; i++) {
                if (states[i].name == name) {
                    return i;
                }
            }
            return -1;
        }

        public bool HasState(string state) {
            return GetStateIndex(state) >= 0;
        }

        private bool IsStateBlocked(int stateIndex) {
            if (states[stateIndex].blockedBy == 0) return false;

            for (int i = 0; i < states.Length; i++) {
                if (IsStateBlockedBy(stateIndex, i)) {
                    return true;
                }
            }

            return false;
        }

        private bool IsStateBlockedBy(int state, int blockingState) {
            return state != blockingState && 
                states[blockingState].isActive && 
                !states[blockingState].isBlocked && 
                (states[state].blockedBy & (1 << blockingState)) != 0;
        }

        public bool IsStateActive(int stateIndex) {
            return states[stateIndex].isActive;
        }

        public bool IsStateActive(string state) {
            int index = GetStateIndex(state);
            if (index < 0) return false;
            return IsStateActive(index);
        }

        public void SetStateActive(int stateIndex, bool active) {
            if (states[stateIndex].isActive == active) return;

            UpdateActiveAndBlocked(stateIndex, active, IsStateBlocked(stateIndex));
        }

        private void UpdateActiveAndBlocked(int stateIndex, bool active, bool isBlocked) {
            bool fieldStatusChanging = (active && !isBlocked) != (states[stateIndex].isActive && !states[stateIndex].isBlocked);

            states[stateIndex].isBlocked = isBlocked;
            states[stateIndex].isActive = active;

            if (fieldStatusChanging) {
                for (int i = 0; i < states[stateIndex].fieldIDs.Length; i++) {
                    if (active && !isBlocked) {
                        ActivateField(stateIndex, i);
                    } else {
                        DeactivateField(stateIndex, i);
                    }
                }
            }

            for (int i = 0; i < states.Length; i++) {
                if (i != stateIndex && 
                    states[i].isActive && 
                    IsStateBlockedBy(i, stateIndex) != states[i].isBlocked &&
                    IsStateBlocked(i) != states[i].isBlocked) {
                    UpdateActiveAndBlocked(i, states[i].isActive, !states[i].isBlocked);
                }
            }
        }

        public void SetStateActive(string state, bool active) {
            int stateIndex = GetStateIndex(state);
            if (stateIndex >= 0) {
                SetStateActive(stateIndex, active);
            }
        }

        private int GetFirstStateControllingField(int fieldID, int excludeState, out SerializedValue value) {
            for (int i = 0; i < states.Length; i++) {
                if (i == excludeState || !states[i].isActive || states[i].isBlocked) continue;

                int index = Array.IndexOf(states[i].fieldIDs, fieldID);
                if (index >= 0) {
                    value = states[i].values.overrides[index];
                    return i;
                }
            }

            value = null;
            return -1;
        }

        private void ActivateField(int stateIndex, int localFieldIndex) {
            int fieldID = states[stateIndex].fieldIDs[localFieldIndex];
            int previous = GetFirstStateControllingField(fieldID, stateIndex, out _);

            if (previous >= 0 && previous < stateIndex) return;

            var valueOverride = states[stateIndex].values.overrides[localFieldIndex];
            if (previous < 0) {
                fieldDefaultValues[fieldID] = valueOverride.GetFieldValue(owner);
            }
            valueOverride.SetFieldValue(owner);
        }

        private void DeactivateField(int stateIndex, int localFieldIndex) {
            int fieldID = states[stateIndex].fieldIDs[localFieldIndex];
            int previous = GetFirstStateControllingField(fieldID, stateIndex, out var previousValueOverride);

            if (previous >= 0 && previous < stateIndex) return;

            var valueOverride = states[stateIndex].values.overrides[localFieldIndex];
            if (previous < 0) {
                valueOverride.SetFieldValue(owner, fieldDefaultValues[fieldID]);
            } else {
                previousValueOverride.SetFieldValue(owner);
            }
        }
    }
}