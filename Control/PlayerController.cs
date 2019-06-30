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
    public abstract class PlayerController<T> : Controller<T> where T : Channels, new() {
        private Dictionary<string, Action> buttonDown;
        private Dictionary<string, Action> buttonUp;
        private Dictionary<string, Action> buttonHeld;
        private Dictionary<string, (bool raw, Action<float> action)> axes;

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

        protected override void Awake() {
            base.Awake();

            axes = new Dictionary<string, (bool raw, Action<float> action)>();
            buttonDown = new Dictionary<string, Action>();
            buttonUp = new Dictionary<string, Action>();
            buttonHeld = new Dictionary<string, Action>();

            if (initialViewTarget) {
                viewTarget = initialViewTarget;
            } else {
                viewTarget = GetComponentInChildren<ViewTarget>();
            }
        }

        protected void AddAxisListener(string axis, Action<float> listener, bool raw = true, bool overwrite = false) {
            try {
                Input.GetAxis(axis + inputSuffix); // Throw an exception if it doesn't exist.
                if (!overwrite && axes.ContainsKey(axis)) {
                    var tuple = axes[axis];
                    tuple.action += listener;
                    axes[axis] = tuple;
                } else {
                    axes[axis] = (raw, listener);
                }
            } catch {
                Debug.LogError("Error: Axis " + axis + inputSuffix + " does not exist.");
            }
        }

        protected void AddButtonListener(string button, Action listener, bool overwrite = false) {
            try {
                Input.GetButton(button + inputSuffix); // Throw an exception if it doesn't exist.
                if (!overwrite && buttonHeld.ContainsKey(button)) {
                    buttonHeld[button] += listener;
                } else {
                    buttonHeld[button] = listener;
                }
            } catch {
                Debug.LogError("Error: Button " + button + inputSuffix + " does not exist.");
            }
        }

        protected void AddButtonUpListener(string button, Action listener, bool overwrite = false) {
            try {
                Input.GetButton(button + inputSuffix); // Throw an exception if it doesn't exist.
                if (!overwrite && buttonUp.ContainsKey(button)) {
                    buttonUp[button] += listener;
                } else {
                    buttonUp[button] = listener;
                }
            } catch {
                Debug.LogError("Error: Button " + button + inputSuffix + " does not exist.");
            }
        }

        protected void AddButtonDownListener(string button, Action listener, bool overwrite = false) {
            try {
                Input.GetButton(button + inputSuffix); // Throw an exception if it doesn't exist.
                if (!overwrite && buttonDown.ContainsKey(button)) {
                    buttonDown[button] += listener;
                } else {
                    buttonDown[button] = listener;
                }
            } catch {
                Debug.LogError("Error: Button " + button + inputSuffix + " does not exist.");
            }
        }

        protected override void DoInput() {
            if (enabled) {
                foreach (var m in axes) {
                    string key = m.Key + inputSuffix;
                    float f = m.Value.raw ? Input.GetAxisRaw(key) : Input.GetAxis(key);
                    m.Value.action?.Invoke(f);
                }

                foreach (var m in buttonDown) {
                    if (Input.GetButtonDown(m.Key + inputSuffix)) {
                        m.Value?.Invoke();
                    }
                }

                foreach (var m in buttonHeld) {
                    if (Input.GetButton(m.Key + inputSuffix)) {
                        m.Value?.Invoke();
                    }
                }

                foreach (var m in buttonUp) {
                    if (Input.GetButtonUp(m.Key + inputSuffix)) {
                        m.Value?.Invoke();
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