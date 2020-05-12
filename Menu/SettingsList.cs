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

using System;
using UnityEngine;

namespace SBR.Menu {
    public class SettingsList : MonoBehaviour {
        [Serializable]
        public class SettingInfo {
            [SettingReference]
            public string setting;
            public SettingControl control;
        }

        [SettingReference(typeof(int))]
        public string masterControl;
        public SettingControl defaultControl;
        public SettingInfo[] settings;

        private SettingControl[] controls;

        private void Start() {
            controls = new SettingControl[transform.childCount];
            for (int i = 0; i < transform.childCount; i++) {
                controls[i] = transform.GetChild(i).GetComponent<SettingControl>();
                controls[i].setting.ObjValueChanged += v => UpdateInteractible();
            }
            UpdateInteractible();
        }

        private void UpdateInteractible() {
            var master = SettingsManager.GetSetting<int>(masterControl);
            if (master != null) {
                foreach (var ctrl in controls) {
                    var setting = ctrl.setting;
                    ctrl.interactible = setting == master || master.value < 0;
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

            foreach (var info in settings) {
                var prefab = info.control ? info.control : defaultControl;
                if (!prefab) continue;

                var c = (SettingControl)UnityEditor.PrefabUtility.InstantiatePrefab(prefab);
                c.settingKey = info.setting;
                c.transform.SetParent(transform, false);
                c.UpdateUIElement();
            }
        }
#endif
    }
}