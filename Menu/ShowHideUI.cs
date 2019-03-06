using UnityEngine;

namespace SBR.Menu {
    public class ShowHideUI : MonoBehaviour {
        public enum Mode {
            SetActive, AnimationBool
        }
        
        public Mode mode;

        [Conditional("mode", Mode.SetActive)]
        public GameObject targetObject;
        [Conditional("mode", Mode.AnimationBool)]
        public Animator targetAnimator;
        [Conditional("mode", Mode.AnimationBool)]
        public string boolParameter = "Paused";

        private void Reset() {
            targetObject = gameObject;
            targetAnimator = GetComponentInParent<Animator>();
        }

        private bool _show;
        public bool show {
            get => _show;
            set {
                if (_show != value) {
                    _show = value;
                    if (mode == Mode.SetActive) {
                        targetObject.SetActive(_show);
                    } else {
                        targetAnimator.SetBool(boolParameter, _show);
                    }
                }
            }
        }
    }
}