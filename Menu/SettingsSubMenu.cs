using System;
using System.Linq;
using UnityEngine;

namespace SBR.Menu {
    public class SettingsSubMenu : MonoBehaviour {
        private Setting[] settings;

        public bool modified => settings.Any(s => s.modified);

        private void Awake() {
            settings = GetComponentsInChildren<SettingControl>().Select(t => t.setting).ToArray();
        }

        public void Defaults() {
            foreach (var setting in settings) {
                SettingsManager.GetSetting(setting.key).Default();
            }
        }

        public void Revert() {
            foreach (var setting in settings) {
                SettingsManager.GetSetting(setting.key).Load();
            }
        }

        public void Save() {
            foreach (var setting in settings) {
                SettingsManager.GetSetting(setting.key).Save();
            }
        }
    }

}