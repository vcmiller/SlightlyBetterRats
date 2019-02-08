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