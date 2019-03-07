using System.Collections;
using System.Collections.Generic;
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