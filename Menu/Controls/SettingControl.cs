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
    public abstract class SettingControl : MonoBehaviour {
        public Text label;
        
        [SettingReference]
        public string settingKey;
        public Setting setting => SettingsManager.GetSetting(settingKey);

        private Selectable selectable;

        public virtual bool interactible {
            get => selectable.interactable;
            set => selectable.interactable = value;
        }

        public virtual void UpdateUIElement() {
            var s = setting;
            if (label && s != null) {
                label.text = s.displayName;
            }
        }

        protected virtual void OnValidate() {
            UpdateUIElement();
        }

        protected virtual void Awake() {
            selectable = GetComponentInChildren<Selectable>();
            UpdateUIElement();
            setting.ObjValueChanged += SettingValueChanged;
        }

        private void OnDestroy() {
            setting.ObjValueChanged -= SettingValueChanged;
        }

        private void SettingValueChanged(object value) {
            UpdateUIElement();
        }
    }
}