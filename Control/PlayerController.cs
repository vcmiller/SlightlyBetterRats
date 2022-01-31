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
        protected bool MouseGrabbed { get; private set; }
        public bool InputEnabled { get; set; } = true;

        public bool GrabMouse {
            get => _grabMouse;
            set => _grabMouse = value;
        }

        /// <summary>
        /// Whether to hide and confine the mouse cursor automatically.
        /// </summary>
        [Tooltip("Whether to hide and confine the mouse cursor automatically.")]
        [SerializeField] private bool _grabMouse = true;

        /// <summary>
        /// Whether to control the ViewTarget's enabled state (if true, don't control).
        /// </summary>
        [Tooltip("If true, don't control the ViewTarget's enabled state.")]
        [SerializeField] private bool _viewTargetIsShared;

        /// <summary>
        /// Initial ViewTarget to use for movement and view.
        /// </summary>
        [Tooltip("Initial ViewTarget to use for movement and view.")]
        [SerializeField] private ViewTarget _initialViewTarget;

        private ViewTarget _viewTarget;

        /// <summary>
        /// ViewTarget to use for movement and view.
        /// </summary>
        public ViewTarget ViewTarget {
            get {
                return _viewTarget;
            }

            set {
                if (_viewTarget && !_viewTargetIsShared) {
                    _viewTarget.enabled = false;
                }
                _viewTarget = value;
                if (_viewTarget && !_viewTargetIsShared) {
                    _viewTarget.enabled = enabled;
                }
            }
        }

        protected override void Awake() {
            base.Awake();

            ViewTarget = _initialViewTarget ? _initialViewTarget : GetComponentInChildren<ViewTarget>();
        }

        protected virtual void OnDisable() {
            if (_viewTarget && !_viewTargetIsShared) {
                _viewTarget.enabled = false;
            }

            if (_grabMouse) {
                ReleaseMouse();
            }
        }

        protected virtual void OnEnable() {
            if (_grabMouse) {
                CaptureMouse();
            }

            if (_viewTarget && !_viewTargetIsShared) {
                _viewTarget.enabled = true;
            }
        }

        protected void CaptureMouse() {
            MouseGrabbed = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        protected void ReleaseMouse() {
            MouseGrabbed = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}