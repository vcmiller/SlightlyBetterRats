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

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SBR.Menu {
    public abstract class SettingControl : MonoBehaviour {
        [SerializeField] private TMP_Text _label;
        [SerializeField] private TMP_Text _descriptionLabel;
        [SettingReference, SerializeField] private string _settingKey;

        public string SettingKey => _settingKey;
        public ISetting Setting => SettingsManager.GetSetting(_settingKey);
        private Selectable _selectable;

        public virtual bool Interactable {
            get => _selectable.interactable;
            set => _selectable.interactable = value;
        }

        public virtual void UpdateUIElement() {
            ISetting s = Setting;
            if (s == null) return;

            if (_label != null) {
                _label.text = s.DisplayName;
            }

            if (_descriptionLabel != null) {
                _descriptionLabel.text = s.Description;
            }
        }

        protected virtual void OnValidate() {
            if (!gameObject.activeInHierarchy) return;
            UpdateUIElement();
        }

        protected virtual void Awake() {
            _selectable = GetComponentInChildren<Selectable>();
            UpdateUIElement();
            Setting.ValueChanged += SettingValueChanged;
        }

        private void OnDestroy() {
            Setting.ValueChanged -= SettingValueChanged;
        }

        private void SettingValueChanged(object value) {
            UpdateUIElement();
        }
    }
}
