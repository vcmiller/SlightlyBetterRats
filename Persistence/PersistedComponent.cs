using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SBR.Persistence {
    public interface IPersistedComponent {
        public void Initialize(PersistedDataBase parent, string id);
    }
    
    public class PersistedComponent<T> : MonoBehaviour, IPersistedComponent where T : PersistedDataBase, new() {
        protected T State { get; private set; }
        
        public void Initialize(PersistedDataBase parent, string id) {
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
    }
}