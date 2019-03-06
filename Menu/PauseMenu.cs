using UnityEngine;

namespace SBR.Menu {
    [RequireComponent(typeof(ShowHideUI))]
    public class PauseMenu : MonoBehaviour {
        private ShowHideUI target;

        private void Awake() {
            target = GetComponent<ShowHideUI>();
        }

        private void OnEnable() {
            Pause.GamePaused += Paused;
        }

        private void OnDisable() {
            Pause.GameResumed += Unpaused;
        }

        private void Paused() => target.show = true;
        private void Unpaused() => target.show = false;
    }

}