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