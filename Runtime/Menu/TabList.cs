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

namespace SBR.Menu {
    public class TabList : MonoBehaviour {
        [FormerlySerializedAs("buttonPrefab")]
        public TabButton _buttonPrefab;
        [FormerlySerializedAs("tabs")]
        public ShowHideUI[] _tabs;
        [FormerlySerializedAs("currentTab")]
        public int _currentTab;

        [FormerlySerializedAs("scrollAxis")]
        public string _scrollAxis;
        [FormerlySerializedAs("repeatDelay")]
        public float _repeatDelay = 0.5f;

        private TabButton[] _buttons;
        private CooldownTimer _scrollTimer;

        private void UpdateActiveTab(int tab) {
            _currentTab = tab;
            for (int i = 0; i < _tabs.Length; i++) {
                _tabs[i].Show = i == _currentTab;
                _buttons[i].IsOn = i == _currentTab;
            }
        }

        private void Awake() {
            _scrollTimer = new CooldownTimer(_repeatDelay);
            _buttons = new TabButton[transform.childCount];
            for (int i = 0; i < transform.childCount; i++) {
                int cpy = i;
                _buttons[i] = transform.GetChild(i).GetComponent<TabButton>();
                _buttons[i]._button.onClick.AddListener(() => UpdateActiveTab(cpy));
            }
        }

        private void Start() {
            UpdateActiveTab(_currentTab);
        }

        private void Update() {
            if (!string.IsNullOrEmpty(_scrollAxis)) {
                float f = Input.GetAxis(_scrollAxis);
                if (f != 0 && _scrollTimer.Use()) {
                    int newTab = _currentTab + (int)Mathf.Sign(f);
                    newTab = (newTab + _tabs.Length) % _tabs.Length;
                    UpdateActiveTab(newTab);
                } else if (f == 0) {
                    _scrollTimer.Clear();
                }
            }
        }

#if UNITY_EDITOR
        public void ClearUI() {
            while (transform.childCount > 0) {
                DestroyImmediate(transform.GetChild(0).gameObject);
            }
        }

        public void UpdateUI() {
            ClearUI();

            for (int i = 0; i < _tabs.Length; i++) {
                ShowHideUI tab = _tabs[i];
                TabButton b = (TabButton)UnityEditor.PrefabUtility.InstantiatePrefab(_buttonPrefab);
                if (b._label) b._label.text = tab.name;
                b.transform.SetParent(transform, false);
            }
        }
#endif
    }

}
