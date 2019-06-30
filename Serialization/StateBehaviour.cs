using System;
using UnityEngine;

namespace SBR.Serialization {
    public interface IStateBehaviour {
        Type type { get; }
        bool HasState(string name);
        bool IsStateActive(string name);
        void SetStateActive(string name, bool value);
    }

    public class StateBehaviour : MonoBehaviour, IStateBehaviour {
        [SerializeField] protected StateList states;

        public Type type => GetType();

        protected virtual void Awake() {
            states.Initialize(this);
        }

        public bool HasState(string name) => states.HasState(name);
        public bool IsStateActive(string name) => states.IsStateActive(name);
        public void SetStateActive(string name, bool value) => states.SetStateActive(name, value);
    }
}