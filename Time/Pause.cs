using System;
using UnityEngine;

namespace SBR {
    /// <summary>
    /// Manages pausing and unpausing of the game.
    /// </summary>
    /// <remarks>
    /// Any actions that should only happen when the game is not paused should check Pause.paused.
    /// Controllers and motors will not have DoInput, DoOutput, and PostOutput called if paused.
    /// Can be used statically if pause is controlled elsewhere,
    /// or placed as a component to automatically pause when a button is pressed.
    /// For creating a pause menu, see PauseMenu.cs.
    /// </remarks>
    public class Pause : MonoBehaviour {
        private static bool _paused;
        private static float timeScale;

        public static event Action GamePaused;
        public static event Action GameResumed;

        public string pauseButton = "Cancel";
        public string resumeButton = "Cancel";

        private void Update() {
            if (!string.IsNullOrEmpty(pauseButton) && 
                Input.GetButtonDown(pauseButton) && !paused) {
                paused = true;
            } else if (!string.IsNullOrEmpty(resumeButton) &&
                Input.GetButtonDown(resumeButton) && paused) {
                paused = false;
            }
        }

        /// <summary>
        /// Controls paused state of the game. 
        /// This cannot completely prevent game actions from happening, but it does the following:
        /// - Sets Time.timeScale to 0 so that Physics and animation will stop.
        /// - Disables all Motors and Controllers from updating.
        /// </summary>
        public static bool paused {
            get => _paused;
            set {
                if (_paused != value) {
                    _paused = value;
                    if (value) {
                        timeScale = Time.timeScale;
                        Time.timeScale = 0;
                        GamePaused?.Invoke();
                    } else {
                        Time.timeScale = timeScale;
                        GameResumed?.Invoke();
                    }
                }
            }
        }
    }

}