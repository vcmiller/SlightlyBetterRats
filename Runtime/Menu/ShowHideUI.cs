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

using Infohazard.Core;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace SBR.Menu {
    public class ShowHideUI : MonoBehaviour {
        public enum Mode {
            SetActive, AnimationBool, CanvasGroup
        }

        [FormerlySerializedAs("mode")]
        public Mode _mode;

        [FormerlySerializedAs("targetObject"),ConditionalDraw(nameof(_mode), Mode.SetActive)]
        public GameObject _targetObject;
        [FormerlySerializedAs("targetAnimator"),ConditionalDraw(nameof(_mode), Mode.AnimationBool)]
        public Animator _targetAnimator;
        [FormerlySerializedAs("boolParameter"),ConditionalDraw(nameof(_mode), Mode.AnimationBool)]
        public string _boolParameter = "Paused";
        [FormerlySerializedAs("canvasGroup"),ConditionalDraw(nameof(_mode), Mode.CanvasGroup)]
        public CanvasGroup _canvasGroup;

        [FormerlySerializedAs("initialSelection")]
        public Selectable _initialSelection;

        private bool _initialized = false;

        private void Reset() {
            _targetObject = gameObject;
            _targetAnimator = GetComponentInParent<Animator>();
            _initialSelection = GetComponentInChildren<Selectable>();
            _canvasGroup = GetComponentInParent<CanvasGroup>();
        }

        private void OnEnable() {
            Initialize();
        }

        private void Initialize() {
            if (_initialized) return;
            _initialized = true;

            if (_initialSelection) {
                _initialSelection.Select();
            }

            if (_mode == Mode.SetActive) {
                _show = _targetObject.activeSelf;
            } else if (_mode == Mode.AnimationBool) {
                _show = _targetAnimator.GetBool(_boolParameter);
            } else {
                _show = _canvasGroup.alpha > 0;
            }
        }

        private bool _show;
        public bool Show {
            get => _show;
            set {
                Initialize();
                if (_show != value) {
                    _show = value;
                    if (_mode == Mode.SetActive) {
                        _targetObject.SetActive(_show);
                    } else if (_mode == Mode.AnimationBool) {
                        _targetAnimator.SetBool(_boolParameter, _show);
                    } else {
                        _canvasGroup.alpha = _show ? 1 : 0;
                        _canvasGroup.blocksRaycasts = Show;
                        _canvasGroup.interactable = Show;
                    }

                    if (_show && _initialSelection) {
                        _initialSelection.Select();
                    }
                }
            }
        }
    }
}
