using UnityEngine.UI;

namespace SBR.Menu {
    public class SettingSlider : SettingControl {
        public Slider slider;

        private void Reset() {
            slider = GetComponentInChildren<Slider>();
        }

        public override void UpdateUIElement() {
            base.UpdateUIElement();
            if (!slider) return;
            switch (setting) {
                case null: break;
                case Setting<int> i:
                    slider.minValue = i.possibleValues[0];
                    slider.maxValue = i.possibleValues[1];
                    slider.value = i.value;
                    slider.wholeNumbers = true;
                    break;
                case Setting<float> f:
                    slider.minValue = f.possibleValues[0];
                    slider.maxValue = f.possibleValues[1];
                    slider.value = f.value;
                    slider.wholeNumbers = false;
                    break;
            }
        }

        protected override void Awake() {
            base.Awake();
            slider.onValueChanged.AddListener(Slider_ValueChanged);
        }

        private void Slider_ValueChanged(float value) {
            switch (setting) {
                case null: break;
                case Setting<int> i:
                    i.value = (int)slider.value;
                    break;
                case Setting<float> f:
                    f.value = slider.value;
                    break;
            }
        }
    }
}