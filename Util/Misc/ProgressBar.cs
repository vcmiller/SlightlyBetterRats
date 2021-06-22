using System;
using System.Collections;
using System.Collections.Generic;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace SBR {
    public class ProgressBar : MonoBehaviour {
        [SerializeField] private Image _fillImage;
        [SerializeField] private float _fillAmount = 0.5f;
        [SerializeField] private TMP_Text _percentText;

        private RectTransform _fillRect;

        public float FillAmount {
            get => _fillAmount;
            set {
                if (_fillAmount == value) return;
                _fillAmount = value;
                UpdateUI();
            }
        }

        private void UpdateUI() {
            if (_fillImage == null) return;
            if (_fillRect == null || _fillRect != _fillImage.transform) {
                _fillRect = _fillImage.GetComponent<RectTransform>();
            }

            if (!_fillRect) return;
            
            if (_fillImage.type == Image.Type.Filled) {
                _fillImage.fillAmount = _fillAmount;
                _fillRect.anchorMax = new Vector2(1, 1);
            } else {
                _fillImage.fillAmount = 1;
                _fillRect.anchorMax = new Vector2(_fillAmount, 1);
            }

            if (_percentText) _percentText.text = $"{_fillAmount}%";
        }

        private void OnValidate() {
            UpdateUI();
        }
    }
}