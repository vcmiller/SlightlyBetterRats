using SBR.Internal;
using SBR.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SBR {
    /// <summary>
    /// An output component that takes input from enabled Controllers on this or parent GameObjects.
    /// Will only take input from compatible Controllers.
    /// (Controllers whose channels type is assignable to the Motor's channels type).
    /// </summary>
    /// <typeparam name="T">The channels type.</typeparam>
    public abstract class Motor<T> : StateBehaviour where T : Channels, new() {
        /// <summary>
        /// Called when enableInput is changed.
        /// </summary>
        public event Action<bool> EnableInputChanged;

        private bool _enableInput = true;
        /// <summary>
        /// Allows enabling/disabling DoOutput callback without disabling the entire Component.
        /// </summary>
        public bool enableInput {
            get => _enableInput;
            set {
                if (_enableInput != value) {
                    _enableInput = value;
                    EnableInputChanged?.Invoke(value);
                }
            }
        }

        /// <summary>
        /// Whether motor currently receives input from any Controller.
        /// </summary>
        public bool receivingInput => enableInput && controllers.Any(c => c.enabled);

        private IController<T>[] controllers;

        /// <summary>
        /// The last Channels object passed to DoOutput.
        /// This may be null! Use the passed-in object where possible.
        /// </summary>
        protected T lastChannels { get; private set; }

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
            controllers = GetComponentsInParent<IController>().OfType<IController<T>>().ToArray();
        }

        private void ConnectControllers() {
            foreach (var ctrl in controllers) {
                ctrl.InputReceived += ControllerInputReceived;
                ctrl.PostInputReceived += ControllerPostInputReceived;
            }
        }

        private void DisconnectControllers() {
            foreach (var ctrl in controllers) {
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
            if (Pause.paused) return;

            lastChannels = channels;
            if (enableInput) {
                try {
                    DoOutput(channels);
                } catch (Exception ex) {
                    Debug.LogException(ex, this);
                }
            }
        }

        private void ControllerPostInputReceived(T channels) {
            if (Pause.paused) return;

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

    public static class MotorExtensions {
        public static IEnumerable<Behaviour> GetMotors(this GameObject obj) {
            return obj.GetComponentsInChildren<Internal.IMotor>().OfType<Behaviour>();
        }
    }
}

namespace SBR.Internal {
    public interface IMotor { }
}