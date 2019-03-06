using UnityEngine.UI;

namespace SBR.Menu {
    public class SettingToggle : SettingControl {
        public Toggle toggle;

        private void Reset() {
            toggle = GetComponentInChildren<Toggle>();
        }

        public override void UpdateUIElement() {
            base.UpdateUIElement();
            if (!toggle || string.IsNullOrEmpty(settingKey) || setting == null) return;
            toggle.isOn = (bool)setting.objValue;
        }
        
        protected override void Awake() {
            base.Awake();
            toggle.onValueChanged.AddListener(Dropdown_ValueChanged);
        }

        private void Dropdown_ValueChanged(bool value) {
            setting.objValue = value;
        }
    }

}