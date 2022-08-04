using UnityEngine;

namespace SBR {
    public class TimeScaleSlider : MonoBehaviour {
        [Range(0, 1)] [SerializeField] private float _timeScale = 1;

        private float _lastTimeScale;

        private void Start() {
            _lastTimeScale = _timeScale;
        }

        private void LateUpdate() {
            if (_timeScale != _lastTimeScale) {
                Time.timeScale = _timeScale;
                Time.fixedDeltaTime = _timeScale / 60.0f;
            }

            _lastTimeScale = _timeScale = Time.timeScale;
        }
    }
}