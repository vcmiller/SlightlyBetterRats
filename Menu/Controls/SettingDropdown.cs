using System;
using System.Linq;
using UnityEngine.UI;

namespace SBR.Menu {
    public class SettingDropdown : SettingControl {
        public Dropdown dropdown;
        private object[] settingValues;

        private void Reset() {
            dropdown = GetComponentInChildren<Dropdown>();
        }

        public override void UpdateUIElement() {
            base.UpdateUIElement();
            if (!dropdown || string.IsNullOrEmpty(settingKey) || setting == null) return;
            settingValues = setting.objPossibleValues.ToArray();
            dropdown.ClearOptions();
            dropdown.AddOptions(settingValues.Select(v => setting.ObjValueToString(v)).ToList());
            dropdown.value = Array.IndexOf(settingValues, setting.objValue);
        }

        protected override void Awake() {
            base.Awake();
            dropdown.onValueChanged.AddListener(Dropdown_ValueChanged);
        }

        private void Dropdown_ValueChanged(int value) {
            setting.objValue = settingValues[value];
        }
    }

}