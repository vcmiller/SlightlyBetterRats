using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace SBR {
    /// <summary>
    /// Used to get input from a player using the Unity input system.
    /// </summary>
    /// <example>
    /// To write your PlayerController, create methods in the following format.
    /// These functions are called automatically.
    /// <code>
    /// public void Axis_Horizontal(float value) { }
    /// public void Button_Fire1() { }
    /// public void ButtonDown_Fire1() { }
    /// public void ButtonUp_Fire1() { }
    /// </code>
    /// </example>
    /// <typeparam name="T">The type of Channels to use.</typeparam>
    public abstract class PlayerController<T> : Controller<T> where T : Channels, new() {
        private Dictionary<string, Action> buttonDown;
        private Dictionary<string, Action> buttonUp;
        private Dictionary<string, Action> buttonHeld;
        private Dictionary<string, Action<float>> axes;

        /// <summary>
        /// Suffix to apply to input axis/button names. 
        /// </summary>
        /// <remarks>
        /// This can be used for local multiplayer games.
        /// For example, there could be multiple characters using the same input code with suffixes set to _1 and _2.
        /// They would then use movement axes Vertical_1/Horizontal_1 and Vertical_2/Horizontal_2 respectively.
        /// </remarks>
        [Tooltip("Suffix to apply to input axis/button names.")]
        public string inputSuffix;

        /// <summary>
        /// Whether to hide and confine the mouse cursor automatically.
        /// </summary>
        [Tooltip("Whether to hide and confine the mouse cursor automatically.")]
        public bool grabMouse = true;

        /// <summary>
        /// Whether invalid axis/button function names should be logged.
        /// </summary>
        [Tooltip("Whether invalid axis/button function names should be logged.")]
        public bool logBadAxes = true;

        /// <summary>
        /// Whether to control the ViewTarget's enabled state (if true, don't control).
        /// </summary>
        [Tooltip("If true, don't control the ViewTarget's enabled state.")]
        public bool sharedViewTarget;

        /// <summary>
        /// Initial ViewTarget to use for movement and view.
        /// </summary>
        [Tooltip("Initial ViewTarget to use for movement and view.")]
        public ViewTarget initialViewTarget;
        
        private ViewTarget _viewTarget;

        /// <summary>
        /// ViewTarget to use for movement and view.
        /// </summary>
        public ViewTarget viewTarget {
            get {
                return _viewTarget;
            }

            set {
                if (_viewTarget && !sharedViewTarget) {
                    _viewTarget.enabled = false;
                }
                _viewTarget = value;
                if (_viewTarget && !sharedViewTarget) {
                    _viewTarget.enabled = enabled;
                }
            }
        }

        protected virtual void Awake() {
            axes = new Dictionary<string, Action<float>>();
            buttonDown = new Dictionary<string, Action>();
            buttonUp = new Dictionary<string, Action>();
            buttonHeld = new Dictionary<string, Action>();

            if (initialViewTarget) {
                viewTarget = initialViewTarget;
            } else {
                viewTarget = GetComponentInChildren<ViewTarget>();
            }

            foreach (MethodInfo m in GetType().GetMethods()) {
                if (m.Name.StartsWith("Axis_")) {
                    var param = m.GetParameters();

                    if (param.Length == 1 && param[0].ParameterType == typeof(float)) {
                        string axis = m.Name.Substring(5);
                        if (axes.ContainsKey(axis)) {
                            Debug.LogWarning("Waring: Duplicate event handler found for axis " + axis + ".");
                        } else {
                            axes.Add(axis, (Action<float>)Delegate.CreateDelegate(typeof(Action<float>), this, m));
                        }
                    } else {
                        Debug.LogWarning("Warning: Axis event handler " + m.Name + " should take one argument of type float.");
                    }
                } else if (m.Name.StartsWith("Button_")) {
                    if (m.GetParameters().Length == 0) {
                        string btn = m.Name.Substring(7);
                        if (buttonHeld.ContainsKey(btn)) {
                            Debug.LogWarning("Waring: Duplicate event handler found for button " + btn + ".");
                        } else {
                            buttonHeld.Add(btn, (Action)Delegate.CreateDelegate(typeof(Action), this, m));
                        }
                    } else {
                        Debug.LogWarning("Warning: Button event handler " + m.Name + " should take no arguments.");
                    }
                } else if (m.Name.StartsWith("ButtonUp_")) {
                    if (m.GetParameters().Length == 0) {
                        string btn = m.Name.Substring(9);
                        if (buttonUp.ContainsKey(btn)) {
                            Debug.LogWarning("Waring: Duplicate event handler found for button up " + btn + ".");
                        } else {
                            buttonUp.Add(btn, (Action)Delegate.CreateDelegate(typeof(Action), this, m));
                        }
                    } else {
                        Debug.LogWarning("Warning: ButtonUp event handler " + m.Name + " should take no arguments.");
                    }
                } else if (m.Name.StartsWith("ButtonDown_")) {
                    if (m.GetParameters().Length == 0) {
                        string btn = m.Name.Substring(11);
                        if (buttonDown.ContainsKey(btn)) {
                            Debug.LogWarning("Waring: Duplicate event handler found for button down " + btn + ".");
                        } else {
                            buttonDown.Add(btn, (Action)Delegate.CreateDelegate(typeof(Action), this, m));
                        }
                    } else {
                        Debug.LogWarning("Warning: ButtonDown event handler " + m.Name + " should take no arguments.");
                    }
                }
            }
        }

        protected override void DoInput() {
            if (enabled) {
                foreach (var m in axes) {
                    try {
                        m.Value(Input.GetAxis(m.Key + inputSuffix));
                    } catch (UnityException ex) {
                        if (logBadAxes) Debug.LogException(ex, this);
                    }
                }

                foreach (var m in buttonDown) {
                    try {
                        if (Input.GetButtonDown(m.Key + inputSuffix)) {
                            m.Value();
                        }
                    } catch (UnityException ex) {
                        if (logBadAxes) Debug.LogException(ex, this);
                    }
                }

                foreach (var m in buttonHeld) {
                    try {
                        if (Input.GetButton(m.Key + inputSuffix)) {
                            m.Value();
                        }
                    } catch (UnityException ex) {
                        if (logBadAxes) Debug.LogException(ex, this);
                    }
                }

                foreach (var m in buttonUp) {
                    try {
                        if (Input.GetButtonUp(m.Key + inputSuffix)) {
                            m.Value();
                        }
                    } catch (UnityException ex) {
                        if (logBadAxes) Debug.LogException(ex, this);
                    }
                }
            }
        }

        protected virtual void OnDisable() {
            if (_viewTarget && !sharedViewTarget) {
                _viewTarget.enabled = false;
            }

            if (grabMouse) {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        protected virtual void OnEnable() {
            if (grabMouse) {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            if (_viewTarget && !sharedViewTarget) {
                _viewTarget.enabled = true;
            }
        }
    }
}