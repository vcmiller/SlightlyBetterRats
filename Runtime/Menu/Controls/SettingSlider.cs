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

using UnityEngine;
using UnityEngine.UI;

namespace SBR.Menu {
    public class SettingSlider : SettingControl {
        [SerializeField] private Slider _slider;

        private void Reset() {
            _slider = GetComponentInChildren<Slider>();
        }

        public override void UpdateUIElement() {
            base.UpdateUIElement();
            if (!_slider) return;
            switch (Setting) {
                case null: break;
                case Setting<int> i:
                    _slider.minValue = i.PossibleValues[0];
                    _slider.maxValue = i.PossibleValues[1];
                    _slider.value = i.Value;
                    _slider.wholeNumbers = true;
                    break;
                case Setting<float> f:
                    _slider.minValue = f.PossibleValues[0];
                    _slider.maxValue = f.PossibleValues[1];
                    _slider.value = f.Value;
                    _slider.wholeNumbers = false;
                    break;
            }
        }

        protected override void Awake() {
            base.Awake();
            _slider.onValueChanged.AddListener(Slider_ValueChanged);
        }

        private void Slider_ValueChanged(float value) {
            switch (Setting) {
                case null: break;
                case Setting<int> i:
                    i.Value = (int)_slider.value;
                    break;
                case Setting<float> f:
                    f.Value = _slider.value;
                    break;
            }
        }
    }
}