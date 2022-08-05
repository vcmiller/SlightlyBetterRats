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
    [RequireComponent(typeof(Button))]
    public class TabButton : MonoBehaviour {
        public Button button;
        public Text label;
        public Image fill;
        
        public Color onLabelColor = Color.black;
        public Color onImageColor = Color.green;

        public Color offLabelColor = Color.black;
        public Color offImageColor = Color.white;

        private bool _isOn;
        public bool isOn {
            get => _isOn;
            set {
                if (_isOn != value) {
                    _isOn = value;
                    if (label) label.color = _isOn ? onLabelColor : offLabelColor;
                    if (fill) fill.color = _isOn ? onImageColor : offImageColor;
                }
            }
        }

        private void Reset() {
            button = GetComponent<Button>();
            fill = GetComponent<Image>();
            label = GetComponentInChildren<Text>();
        }
    }

}