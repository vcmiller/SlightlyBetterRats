using System;
using UnityEngine;
using UnityEngine.Audio;

namespace SBR.Menu {
    public class AudioVolumeUpdater : MonoBehaviour {
        [Serializable]
        public class Info {
            [SettingReference(typeof(float))]
            public string setting;
            public AudioMixer mixer;
            public string nameOverride;

            [NonSerialized]
            public Action<float> listener;
        }

        public Info[] settings;

        private void OnEnable() {
            foreach (var item in settings) {
                var setting = SettingsManager.GetSetting<float>(item.setting);
                if (setting == null) continue;
                item.listener = v => UpdateSettings(item, setting, v);
                setting.ValueChanged += item.listener;
                UpdateSettings(item, setting, setting.value);
            }
        }

        private void Start() {
            foreach (var item in settings) {
                var setting = SettingsManager.GetSetting<float>(item.setting);
                if (setting == null) continue;
                UpdateSettings(item, setting, setting.value);
            }
        }

        private void OnDisable() {
            foreach (var item in settings) {
                var setting = SettingsManager.GetSetting<float>(item.setting);
                if (setting == null) continue;
                setting.ValueChanged -= item.listener;
            }
        }

        private void UpdateSettings(Info item, Setting setting, float val) {
            float log = Mathf.Log(Mathf.Clamp(val, 0.001f, 1.0f)) * 20;

            string name = item.nameOverride;
            if (string.IsNullOrEmpty(name)) {
                name = setting.name;
            }

            item.mixer.SetFloat(name, log);
        }
    }
}