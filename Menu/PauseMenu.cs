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

using UnityEngine;

namespace SBR.Menu {
    public class PauseMenu : MonoBehaviour {
        public ShowHideUI target;
        public bool ungrabMouse = true;

        public string pauseButton = "Cancel";
        public string resumeButton = "Cancel";

        private CursorLockMode cursorLockState;
        private bool cursorVisible;

        private void Reset() {
            target = GetComponentInChildren<ShowHideUI>();
        }

        private void Awake() {
            if (target) target.show = false;
        }

        private void Update() {
            if (!string.IsNullOrEmpty(pauseButton) &&
                Input.GetButtonDown(pauseButton) && 
                !Pause.paused && (!target || !target.show)) {
                Pause.paused = true;
                Paused();
            } else if (!string.IsNullOrEmpty(resumeButton) &&
                Input.GetButtonDown(resumeButton) && 
                Pause.paused && (!target || target.show)) {
                Pause.paused = false;
                Unpaused();
            }
        }

        private void Paused() {
            if (target) target.show = true;
            if (ungrabMouse) {
                cursorLockState = Cursor.lockState;
                cursorVisible = Cursor.visible;

                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        private void Unpaused() {
            if (target) target.show = false;
            if (ungrabMouse) {
                Cursor.lockState = cursorLockState;
                Cursor.visible = cursorVisible;
            }
        }
    }

}