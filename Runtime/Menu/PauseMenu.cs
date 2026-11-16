// The MIT License (MIT)
//
// Copyright (c) 2026-present Val Miller
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

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace SBR.Menu {
    public class PauseMenu : MonoBehaviour {
        [FormerlySerializedAs("target")]
        public ShowHideUI _target;
        [FormerlySerializedAs("ungrabMouse")]
        public bool _ungrabMouse = true;
        [FormerlySerializedAs("pauseCooldown")]
        public float _pauseCooldown = 0.1f;

        [FormerlySerializedAs("pauseAction")]
        public InputActionReference _pauseAction;
        [FormerlySerializedAs("resumeAction")]
        public InputActionReference _resumeAction;

        [FormerlySerializedAs("onOpen")]
        public UnityEvent _onOpen;
        [FormerlySerializedAs("onClose")]
        public UnityEvent _onClose;

        public bool IsOpen { get; private set; }

        private CursorLockMode _cursorLockState;
        private bool _cursorVisible;
        private CooldownTimer _pauseTimer;

        private void Reset() {
            _target = GetComponentInChildren<ShowHideUI>();
        }

        private void Awake() {
            if (_pauseCooldown > 0) {
                _pauseTimer = new CooldownTimer(_pauseCooldown) {Unscaled = true};
                _pauseTimer.Clear();
            }
            if (_target) _target.Show = false;
            if (_pauseAction) _pauseAction.action.Enable();
            if (_resumeAction) _resumeAction.action.Enable();
            IsOpen = false;
        }

        private void Update() {
            if (_pauseAction != null &&
                _pauseAction.action.WasPerformedThisFrame() &&
                !Infohazard.Core.Pause.Paused &&
                !IsOpen &&
                _pauseTimer.CanUse) {

                Pause();
            } else if (_resumeAction != null &&
                _resumeAction.action.WasPerformedThisFrame() &&
                Infohazard.Core.Pause.Paused &&
                IsOpen &&
                _pauseTimer.CanUse) {

                Unpause();
            }
        }

        public void Pause() {
            if (IsOpen) return;
            IsOpen = true;
            if (Infohazard.Core.Pause.Paused) return;
            Infohazard.Core.Pause.Paused = true;
            _pauseTimer.Reset();
            Paused();
        }

        public void Unpause() {
            if (!IsOpen) return;
            IsOpen = false;
            if (!Infohazard.Core.Pause.Paused) return;
            Infohazard.Core.Pause.Paused = false;
            _pauseTimer.Reset();
            Unpaused();
        }

        private void Paused() {
            if (_target) _target.Show = true;
            _onOpen?.Invoke();
            if (_ungrabMouse) {
                _cursorLockState = Cursor.lockState;
                _cursorVisible = Cursor.visible;

                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        private void Unpaused() {
            if (_target) _target.Show = false;
            _onClose?.Invoke();
            if (_ungrabMouse) {
                Cursor.lockState = _cursorLockState;
                Cursor.visible = _cursorVisible;
            }
        }
    }
}
