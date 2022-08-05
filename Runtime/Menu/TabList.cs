// The MIT License (MIT)
// 
// Copyright (c) 2022-present Vincent Miller
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

namespace SBR.Menu {
    public class TabList : MonoBehaviour {
        public TabButton buttonPrefab;
        public ShowHideUI[] tabs;
        public int currentTab;

        public string scrollAxis;
        public float repeatDelay = 0.5f;

        private TabButton[] buttons;
        private CooldownTimer scrollTimer;

        private void UpdateActiveTab(int tab) {
            currentTab = tab;
            for (int i = 0; i < tabs.Length; i++) {
                tabs[i].show = i == currentTab;
                buttons[i].isOn = i == currentTab;
            }
        }

        private void Awake() {
            scrollTimer = new CooldownTimer(repeatDelay);
            buttons = new TabButton[transform.childCount];
            for (int i = 0; i < transform.childCount; i++) {
                int cpy = i;
                buttons[i] = transform.GetChild(i).GetComponent<TabButton>();
                buttons[i].button.onClick.AddListener(() => UpdateActiveTab(cpy));
            }
        }

        private void Start() {
            UpdateActiveTab(currentTab);
        }

        private void Update() {
            if (!string.IsNullOrEmpty(scrollAxis)) {
                float f = Input.GetAxis(scrollAxis);
                if (f != 0 && scrollTimer.Use()) {
                    int newTab = currentTab + (int)Mathf.Sign(f);
                    newTab = (newTab + tabs.Length) % tabs.Length;
                    UpdateActiveTab(newTab);
                } else if (f == 0) {
                    scrollTimer.Clear();
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

            for (int i = 0; i < tabs.Length; i++) {
                var tab = tabs[i];
                var b = (TabButton)UnityEditor.PrefabUtility.InstantiatePrefab(buttonPrefab);
                if (b.label) b.label.text = tab.name;
                b.transform.SetParent(transform, false);
            }
        }
#endif
    }

}