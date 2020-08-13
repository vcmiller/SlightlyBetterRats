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

        private bool _initialized = false;

        private void Reset() {
            targetObject = gameObject;
            targetAnimator = GetComponentInParent<Animator>();
            initialSelection = GetComponentInChildren<Selectable>();
            canvasGroup = GetComponentInParent<CanvasGroup>();
        }

        private void OnEnable() {
            Initialize();
        }

        private void Initialize() {
            if (_initialized) return;
            _initialized = true;

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
                Initialize();
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