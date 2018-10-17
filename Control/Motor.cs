using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SBR {
    public abstract class Motor : MonoBehaviour {
        public bool enableInput { get; set; }

        protected virtual void Start() {
            enableInput = true;
        }

        public abstract void TakeInput();

        public virtual void UpdateAfterInput() { }
    }
}