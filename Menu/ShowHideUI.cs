using UnityEngine;
using UnityEngine.UI;

namespace SBR.Menu {
    public class ShowHideUI : MonoBehaviour {
        public enum Mode {
            SetActive, AnimationBool, CanvasGroup
        }
        
        public Mode mode;

        [Conditional("mode", Mode.SetActive)]
        public GameObject targetObject;
        [Conditional("mode", Mode.AnimationBool)]
        public Animator targetAnimator;
        [Conditional("mode", Mode.AnimationBool)]
        public string boolParameter = "Paused";
        [Conditional("mode", Mode.CanvasGroup)]
        public CanvasGroup canvasGroup;

        public Selectable initialSelection;

        private void Reset() {
            targetObject = gameObject;
            targetAnimator = GetComponentInParent<Animator>();
            initialSelection = GetComponentInChildren<Selectable>();
            canvasGroup = GetComponentInParent<CanvasGroup>();
        }

        private void OnEnable() {
            if (initialSelection) {
                initialSelection.Select();
            }

            if (mode == Mode.SetActive) {
                _show = targetObject.activeSelf;
            } else if (mode == Mode.AnimationBool) {
                _show = targetAnimator.GetBool(boolParameter);
            } else {
                _show = canvasGroup.alpha > 0;
            }
        }

        private bool _show;
        public bool show {
            get => _show;
            set {
                if (_show != value) {
                    _show = value;
                    if (mode == Mode.SetActive) {
                        targetObject.SetActive(_show);
                    } else if (mode == Mode.AnimationBool) {
                        targetAnimator.SetBool(boolParameter, _show);
                    } else {
                        canvasGroup.alpha = _show ? 1 : 0;
                        canvasGroup.blocksRaycasts = show;
                        canvasGroup.interactable = show;
                    }

                    if (_show && initialSelection) {
                        initialSelection.Select();
                    }
                }
            }
        }
    }
}