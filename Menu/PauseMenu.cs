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
            target.show = false;
        }

        private void Update() {
            if (!string.IsNullOrEmpty(pauseButton) &&
                Input.GetButtonDown(pauseButton) && 
                !Pause.paused && !target.show) {
                Pause.paused = true;
                Paused();
            } else if (!string.IsNullOrEmpty(resumeButton) &&
                Input.GetButtonDown(resumeButton) && 
                Pause.paused && target.show) {
                Pause.paused = false;
                Unpaused();
            }
        }

        private void Paused() {
            target.show = true;
            if (ungrabMouse) {
                cursorLockState = Cursor.lockState;
                cursorVisible = Cursor.visible;

                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        private void Unpaused() {
            target.show = false;
            if (ungrabMouse) {
                Cursor.lockState = cursorLockState;
                Cursor.visible = cursorVisible;
            }
        }
    }

}