// MIT License
// 
// Copyright (c) 2020 Vincent Miller
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

using SBR.StateSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SBR {
    /// <summary>
    /// Interface for Controllers, Components which make decisions for what a GameObject is going to do.
    /// Decisions are passed to output objects (Motors) through a Channels object.
    /// </summary>
    /// <typeparam name="T">The type of Channels object that is used.</typeparam>
    public interface IController<out T> : Internal.IController where T : Channels, new() {
        /// <summary>
        /// Whether controller is enabled.
        /// </summary>
        bool enabled { get; set; }

        /// <summary>
        /// Invoked each frame after Channels have been updated.
        /// </summary>
        event Action<T> InputReceived;

        /// <summary>
        /// Invoked each frame after InputReceived.
        /// </summary>
        event Action<T> PostInputReceived;
    }

    public abstract class Controller<T> : StateBehaviour, IController<T> where T : Channels, new() {
        /// <summary>
        /// The channels object which carries input information.
        /// </summary>
        public T channels { get; private set; }
        private readonly SafeUpdateSet<Action<T>> _inputReceived = new SafeUpdateSet<Action<T>>();
        private readonly SafeUpdateSet<Action<T>> _postInputReceived = new SafeUpdateSet<Action<T>>();

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
        public event Action<T> PostInputReceived {
            add { _postInputReceived.Add(value); }
            remove { _postInputReceived.Remove(value); }
        }

        public Controller() {
            channels = new T();
        }

        private void Update() {
            if (Pause.Paused) return;

            try {
                DoInput();
            } catch (Exception ex) {
                Debug.LogException(ex, this);
            }

            _inputReceived.Update();
            _postInputReceived.Update();

            foreach (var callback in _inputReceived) {
                callback(channels);
            }

            channels.ClearInput();

            foreach (var callback in _postInputReceived) {
                callback(channels);
            }
        }

        protected abstract void DoInput();
    }

    public static class ControllerExtensions {
        public static Behaviour GetController(this GameObject obj) {
            return obj.GetComponentInParent<Internal.IController>() as Behaviour;
        }

        public static IEnumerable<Behaviour> GetControllers(this GameObject obj) {
            return obj.GetComponentsInParent<Internal.IController>().OfType<Behaviour>();
        }
    }
}

namespace SBR.Internal {
    public interface IController { }
}