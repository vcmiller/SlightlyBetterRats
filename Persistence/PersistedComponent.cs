using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SBR.Persistence {
    public interface IPersistedComponent {
        public void Initialize(PersistedData parent, string id);
        public void PostLoad();
        public void WillSaveState();
    }
    
    public class PersistedComponent<T> : MonoBehaviour, IPersistedComponent where T : PersistedData, new() {
        protected T State { get; private set; }
        public bool Initialized => State != null;

        public void Initialize(PersistedData parent, string id) {
            if (PersistenceManager.Instance.GetCustomData(parent, id, out T state)) {
                State = state;
                if (state.Initialized) LoadState();
                else LoadDefaultState();
                State.Initialized = true;
            } else {
                State = null;
                Debug.LogError($"State {id} could not be loaded.");
            }
        }

        protected void CreateFakeState() {
            State = new T();
            LoadDefaultState();
        }
        
        public virtual void LoadState() {}
        public virtual void LoadDefaultState() {}
        public virtual void PostLoad() {}
        public virtual void WillSaveState() {}
    }
}