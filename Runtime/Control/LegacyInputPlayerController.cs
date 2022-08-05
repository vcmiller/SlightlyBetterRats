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

using System;
using System.Collections.Generic;
using UnityEngine;

namespace SBR {
    /// <summary>
    /// Used to get input from a player using the Unity input system.
    /// </summary>
    /// <example>
    /// To write your PlayerController, create listener methods for each desired button/axis
    /// and register them using Add...Listener.
    /// Alternatively, call AutoRegisterListeners and create methods in the following format:
    /// <code>
    /// public void Axis_Horizontal(float value) { }
    /// public void Button_Fire1() { }
    /// public void ButtonDown_Fire1() { }
    /// public void ButtonUp_Fire1() { }
    /// </code>
    /// </example>
    /// <typeparam name="T">The type of Channels to use.</typeparam>
    public abstract class LegacyInputPlayerController<T> : PlayerController<T> where T : Channels, new() {
        private Dictionary<string, Action> _buttonDown;
        private Dictionary<string, Action> _buttonUp;
        private Dictionary<string, Action> _buttonHeld;
        private Dictionary<string, (bool raw, Action<float> action)> _axes;

        private readonly HashSet<string> _keysHeld = new HashSet<string>();

        /// <summary>
        /// Suffix to apply to input axis/button names.
        /// </summary>
        /// <remarks>
        /// This can be used for local multiplayer games.
        /// For example, there could be multiple characters using the same input code with suffixes set to _1 and _2.
        /// They would then use movement axes Vertical_1/Horizontal_1 and Vertical_2/Horizontal_2 respectively.
        /// </remarks>
        [Tooltip("Suffix to apply to input axis/button names.")]
        [SerializeField] private string _inputSuffix;

        protected override void Awake() {
            base.Awake();

            _axes = new Dictionary<string, (bool raw, Action<float> action)>();
            _buttonDown = new Dictionary<string, Action>();
            _buttonUp = new Dictionary<string, Action>();
            _buttonHeld = new Dictionary<string, Action>();
        }

        protected void AddAxisListener(string axis, Action<float> listener, bool raw = true, bool overwrite = false) {
            try {
                Input.GetAxis(axis + _inputSuffix); // Throw an exception if it doesn't exist.
                if (!overwrite && _axes.ContainsKey(axis)) {
                    var tuple = _axes[axis];
                    tuple.action += listener;
                    _axes[axis] = tuple;
                } else {
                    _axes[axis] = (raw, listener);
                }
            } catch {
                Debug.LogError("Error: Axis " + axis + _inputSuffix + " does not exist.");
            }
        }

        protected void AddButtonListener(string button, Action listener, bool overwrite = false) {
            try {
                Input.GetButton(button + _inputSuffix); // Throw an exception if it doesn't exist.
                if (!overwrite && _buttonHeld.ContainsKey(button)) {
                    _buttonHeld[button] += listener;
                } else {
                    _buttonHeld[button] = listener;
                }
            } catch {
                Debug.LogError("Error: Button " + button + _inputSuffix + " does not exist.");
            }
        }

        protected void AddButtonUpListener(string button, Action listener, bool overwrite = false) {
            try {
                Input.GetButton(button + _inputSuffix); // Throw an exception if it doesn't exist.
                if (!overwrite && _buttonUp.ContainsKey(button)) {
                    _buttonUp[button] += listener;
                } else {
                    _buttonUp[button] = listener;
                }
            } catch {
                Debug.LogError("Error: Button " + button + _inputSuffix + " does not exist.");
            }
        }

        protected void AddButtonDownListener(string button, Action listener, bool overwrite = false) {
            try {
                Input.GetButton(button + _inputSuffix); // Throw an exception if it doesn't exist.
                if (!overwrite && _buttonDown.ContainsKey(button)) {
                    _buttonDown[button] += listener;
                } else {
                    _buttonDown[button] = listener;
                }
            } catch {
                Debug.LogError("Error: Button " + button + _inputSuffix + " does not exist.");
            }
        }

        protected override void DoInput() {
            if (enabled) {
                foreach (var m in _axes) {
                    if (InputEnabled) {
                        string key = m.Key + _inputSuffix;
                        float f = m.Value.raw ? Input.GetAxisRaw(key) : Input.GetAxis(key);
                        m.Value.action?.Invoke(f);
                    } else {
                        m.Value.action?.Invoke(0);
                    }
                }

                foreach (var m in _buttonDown) {
                    if (InputEnabled && Input.GetButtonDown(m.Key + _inputSuffix)) {
                        m.Value?.Invoke();
                    }
                }

                foreach (var m in _buttonHeld) {
                    if (InputEnabled && Input.GetButton(m.Key + _inputSuffix)) {
                        m.Value?.Invoke();
                    }
                }

                foreach (var m in _buttonUp) {
                    string key = m.Key + _inputSuffix;
                    if (InputEnabled && Input.GetButtonDown(key)) {
                        _keysHeld.Add(key);
                    }

                    if ((!InputEnabled || Input.GetButtonUp(m.Key + _inputSuffix)) && _keysHeld.Remove(key)) {
                        m.Value?.Invoke();
                    }
                }
            }
        }
    }
}