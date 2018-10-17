using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SBR {
    public abstract class BasicMotor<T> : Motor where T : Channels {
        public T channels { get; private set; }

        protected override void Start() {
            base.Start();
            Brain b = GetComponentInParent<Brain>();
            if (b) {
                channels = b.channels as T;
            }
        }
    }
}
