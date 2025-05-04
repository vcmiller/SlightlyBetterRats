// The MIT License (MIT)
//
// Copyright (c) 2022-present Vincent Miller
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

using SBR.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Infohazard.Core;
using Infohazard.Sequencing;
using Infohazard.StateSystem;
using UnityEngine;

namespace SBR {
    /// <summary>
    /// An output component that takes input from enabled Controllers on this or parent GameObjects.
    /// Will only take input from compatible Controllers.
    /// (Controllers whose channels type is assignable to the Motor's channels type).
    /// </summary>
    /// <typeparam name="T">The channels type.</typeparam>
    public abstract class Motor<T> : StateBehaviour, IMotor where T : Channels, new() {
        /// <summary>
        /// Called when enableInput is changed.
        /// </summary>
        public event Action<bool> EnableInputChanged;

        [SerializeField] private bool _enableInput = true;
        /// <summary>
        /// Allows enabling/disabling DoOutput callback without disabling the entire Component.
        /// </summary>
        public bool EnableInput {
            get => _enableInput;
            set {
                if (_enableInput != value) {
                    _enableInput = value;
                    EnableInputChanged?.Invoke(value);
                }
            }
        }

        protected IController<T>[] Controllers { get; private set; }

        /// <summary>
        /// The last Channels object passed to DoOutput.
        /// This may be null! Use the passed-in object where possible.
        /// </summary>
        protected T LastChannels { get; private set; }

        protected override void Awake() {
            base.Awake();
            GetControllersArray();
        }

        protected virtual void OnEnable() {
            ConnectControllers();
        }

        protected virtual void OnDisable() {
            DisconnectControllers();
        }

        private void GetControllersArray() {
            Controllers = GetComponentsInParent<IController>().OfType<IController<T>>().ToArray();
        }

        private void ConnectControllers() {
            foreach (var ctrl in Controllers) {
                ctrl.InputReceived += ControllerInputReceived;
                ctrl.PostInputReceived += ControllerPostInputReceived;
            }
        }

        private void DisconnectControllers() {
            foreach (var ctrl in Controllers) {
                ctrl.InputReceived -= ControllerInputReceived;
                ctrl.PostInputReceived -= ControllerPostInputReceived;
            }
        }

        public void RefreshControllers() {
            if (enabled) {
                DisconnectControllers();
            }

            GetControllersArray();

            if (enabled) {
                ConnectControllers();
            }
        }

        private void ControllerInputReceived(T channels) {
            if (Pause.Paused) return;

            LastChannels = channels;
            if (EnableInput) {
                try {
                    DoOutput(channels);
                } catch (Exception ex) {
                    Debug.LogException(ex, this);
                }
            }
        }

        private void ControllerPostInputReceived(T channels) {
            if (Pause.Paused) return;

            try {
                PostOutput(channels);
            } catch (Exception ex) {
                Debug.LogException(ex, this);
            }
        }

        /// <summary>
        /// Called after receiving input from the Controller. Use this to read any input information.
        /// This will only be called if enableInput is true.
        /// </summary>
        /// <param name="channels">Channels carrying input information.</param>
        protected abstract void DoOutput(T channels);

        /// <summary>
        /// Called after DoOutput has been called on all Motors. Use this for post-input updates, such as rotating a third-person camera.
        /// This will still be called if enableInput is false.
        /// </summary>
        protected virtual void PostOutput(T channels) { }
    }

    public abstract class PersistedMotor<TChannels, TState> : Motor<TChannels>, IPersistedComponent
        where TState : PersistedData, new()
        where TChannels : Channels, new() {

        protected TState State { get; private set; }
        public bool Initialized => State != null;
        protected PersistedGameObjectBase Owner { get; private set; }

        public void Initialize(PersistedGameObjectBase owner, PersistedData parent, string id) {
            if (PersistenceManager.Instance.GetCustomData(parent, id, out TState state)) {
                Owner = owner;
                State = state;
                if (state.Initialized) LoadState();
                else LoadDefaultState();
                State.Initialized = true;
            } else {
                State = null;
                Debug.LogError($"State {id} could not be loaded.");
            }
        }

        public virtual void LoadState() {}
        public virtual void LoadDefaultState() {}
        public virtual UniTask PostLoad() => UniTask.CompletedTask;
        public virtual void WriteState() {}
    }

    public static class MotorExtensions {
        public static IEnumerable<Behaviour> GetMotors(this GameObject obj) {
            return obj.GetComponentsInChildren<Internal.IMotor>().OfType<Behaviour>();
        }
    }
}

namespace SBR.Internal {
    public interface IMotor { }
}
