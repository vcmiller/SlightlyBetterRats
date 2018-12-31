using System;
using System.Collections.Generic;
using UnityEngine;

namespace SBR {
    /// <summary>
    /// Interface for Controllers, Components which make decisions for what a GameObject is going to do.
    /// Decisions are passed to output objects (Motors) through a Channels object.
    /// </summary>
    /// <typeparam name="T">The type of Channels object that is used.</typeparam>
    public interface IController<out T> : DoNotUse.IController where T : Channels, new() {
        /// <summary>
        /// Invoked each frame after Channels have been updated.
        /// </summary>
        event Action<T> InputReceived;

        /// <summary>
        /// Invoked each frame after InputReceived.
        /// </summary>
        event Action PostInputReceived;
    }

    public abstract class Controller<T> : MonoBehaviour, IController<T> where T : Channels, new() {
        /// <summary>
        /// The channels object which carries input information.
        /// </summary>
        public T channels { get; private set; }
        private readonly HashSet<Action<T>> _inputReceived = new HashSet<Action<T>>();

        /// <summary>
        /// Invoked each frame after Channels have been updated.
        /// </summary>
        public event Action<T> InputReceived {
            add { _inputReceived.Add(value); }
            remove { _inputReceived.Remove(value); }
        }

        /// <summary>
        /// Invoked each frame after InputReceived.
        /// </summary>
        public event Action PostInputReceived;

        protected virtual void Awake() {
            channels = new T();
        }

        private void Update() {
            try {
                DoInput();
            } catch (Exception ex) {
                Debug.LogException(ex, this);
            }

            foreach (var callback in _inputReceived) {
                callback(channels);
            }

            channels.ClearInput();

            PostInputReceived?.Invoke();
        }

        protected abstract void DoInput();
    }
}

namespace SBR.DoNotUse {
    public interface IController { }
}