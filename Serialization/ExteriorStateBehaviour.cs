using System;
using UnityEngine;

namespace SBR.Serialization {
    public class ExteriorStateBehaviour : MonoBehaviour, IStateBehaviour {
        [SerializeField] protected Component component;
        [SerializeField] protected StateList states;

        public Type type => component ? component.GetType() : typeof(Component);

        protected virtual void Awake() {
            states.Initialize(component);
        }

        public bool HasState(string name) => states.HasState(name);
        public bool IsStateActive(string name) => states.IsStateActive(name);
        public void SetStateActive(string name, bool value) => states.SetStateActive(name, value);
    }

}