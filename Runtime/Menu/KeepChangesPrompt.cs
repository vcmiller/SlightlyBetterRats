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
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace SBR.Menu {
    public class KeepChangesPrompt : MonoBehaviour {
        [FormerlySerializedAs("time")]
        public float _time;
        private string _textTemplate;
        [FormerlySerializedAs("text")]
        public Text _text;
        [FormerlySerializedAs("settings")]
        public SettingsSubMenu _settings;
        [FormerlySerializedAs("select")]
        public Selectable _select;

        private ExpirationTimer _timer;

        private void Awake() {
            _textTemplate = _text.text;
        }

        private void OnEnable() {
            _timer = new ExpirationTimer(_time);
            _timer.Set();
            _select.Select();
        }

        private void Update() {
            if (_text) _text.text = string.Format(_textTemplate, Mathf.CeilToInt(_timer.Remaining));
            if (_timer.ExpiredThisFrame) {
                Revert();
            }
        }

        public void Revert() {
            _settings.Revert();
            gameObject.SetActive(false);
        }

        public void Confirm() {
            _settings.Save();
            gameObject.SetActive(false);
        }
    }
}
