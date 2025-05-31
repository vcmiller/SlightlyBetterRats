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

using System.Diagnostics;
using Infohazard.Core;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace SBR {
    public class Healthbar : MonoBehaviour {
        [SerializeField]
        [Tooltip("Target Health that this Healthbar monitors.")]
        private Health _target;

        /// <summary>
        /// Image that is used for the bar itself.
        /// If the mode is set to filled, will control the fill amount.
        /// Otherwise, will control the anchor positions.
        /// </summary>
        [SerializeField]
        [FormerlySerializedAs("fillImage")]
        [Tooltip("Image that is used for the bar itself.")]
        private Image _fillImage;

        /// <summary>
        /// Image that is used for the background of the healthbar.
        /// </summary>
        [SerializeField]
        [FormerlySerializedAs("backImage")]
        [Tooltip("Image that is used for the background of the healthbar.")]
        private Image _backImage;

        [SerializeField]
        [FormerlySerializedAs("backFillImage")]
        [Tooltip("Image that updates in a delayed mode behind the main fill, in order to show damage taken.")]
        private Image _backFillImage;

        [SerializeField]
        [FormerlySerializedAs("backFillDelay")]
        [ConditionalDraw(nameof(_backFillImage), null, false)]
        [Tooltip("How long to wait before the back fill image starts updating after damage is taken.")]
        private float _backFillDelay = 0.5f;

        [SerializeField]
        [FormerlySerializedAs("backFillSpeed")]
        [ConditionalDraw(nameof(_backFillImage), null, false)]
        [Tooltip("How fast the back fill image updates to show damage taken, as a fraction of the bar per second.")]
        private float _backFillSpeed = 1.0f;

        /// <summary>
        /// Text used to show amount.
        /// </summary>
        [SerializeField]
        [FormerlySerializedAs("amountText")]
        [Tooltip("Text used to show amount.")]
        private TMP_Text _amountText;

        /// <summary>
        /// How to display the amount text.
        /// </summary>
        [SerializeField]
        [FormerlySerializedAs("amountTextType")]
        [Tooltip("How to display the amount text.")]
        [ConditionalDraw(nameof(_amountText), null, false)]
        private AmountTextType _amountTextType;

        [SerializeField]
        [FormerlySerializedAs("amountTextFormat")]
        private string _amountTextFormat;

        /// <summary>
        /// Optional additional graphics which will be shown and hidden along with the rest of the Healthbar.
        /// </summary>
        [SerializeField]
        [FormerlySerializedAs("additionalGraphics")]
        [Tooltip("Optional additional graphics which will be shown and hidden along with the rest of the Healthbar.")]
        private Graphic[] _additionalGraphics;

        /// <summary>
        /// Whether to track the location of the target on screen.
        /// </summary>
        [SerializeField]
        [FormerlySerializedAs("trackOnScreen")]
        [Tooltip("Whether to track the location of the target on screen.")]
        private bool _trackOnScreen;

        /// <summary>
        /// Camera to use for tracking on screen. Leave empty to use Camera.main.
        /// </summary>
        [SerializeField]
        [FormerlySerializedAs("trackingCamera")]
        [ConditionalDraw(nameof(_trackOnScreen))]
        [Tooltip("Camera to use for tracking on screen. Leave empty to use Camera.main.")]
        private Camera _trackingCamera;

        /// <summary>
        /// World space offset to apply when tracking the target.
        /// </summary>
        [SerializeField]
        [FormerlySerializedAs("trackWorldOffset")]
        [ConditionalDraw(nameof(_trackOnScreen))]
        [Tooltip("World space offset to apply when tracking the target.")]
        private Vector3 _trackWorldOffset = Vector3.up;

        /// <summary>
        /// Screen space offset to apply when tracking the target.
        /// </summary>
        [SerializeField]
        [FormerlySerializedAs("trackScreenOffset")]
        [ConditionalDraw(nameof(_trackOnScreen))]
        [Tooltip("Screen space offset to apply when tracking the target.")]
        private Vector2 _trackScreenOffset;

        /// <summary>
        /// How the Healthbar is displayed.
        /// </summary>
        [SerializeField]
        [FormerlySerializedAs("displayMode")]
        [Tooltip("How the Healthbar is displayed.")]
        private DisplayMode _displayMode = DisplayMode.Visible;

        /// <summary>
        /// When using DisplayMode.OnDamage, how long to show the Healthbar when it is damaged.
        /// </summary>
        [SerializeField]
        [FormerlySerializedAs("displayDuration")]
        [ConditionalDraw(nameof(_displayMode), DisplayMode.OnDamage, true)]
        [Tooltip("How long to show the Healthbar when it is damaged.")]
        private float _displayDuration = 3;

        /// <summary>
        /// Target Health that this Healthbar monitors.
        /// </summary>
        public Health Target {
            get => _target;
            set {
                if (_target == value) return;
                if (_target) {
                    _target.Damaged -= HealthDamaged;
                }

                _target = value;

                if (_target) {
                    _target.Damaged += HealthDamaged;
                    _backFillAmount = _target.CurrentHealth / _target.maxHealth;
                }
            }
        }

        public Image FillImage {
            get => _fillImage;
            set => _fillImage = value;
        }

        public Image BackImage {
            get => _backImage;
            set => _backImage = value;
        }

        public enum DisplayMode {
            Hidden,
            Visible,
            OnDamage
        }

        public enum AmountTextType {
            Percent,
            Fraction,
            Amount
        }

        private Canvas _canvas;
        private PassiveTimer _displayTimer;
        private PassiveTimer _backFillTimer;
        private float _backFillAmount;

        private void HealthDamaged(Damage dmg) {
            _displayTimer.StartInterval();
            _backFillTimer.StartInterval();
        }

        private void Awake() {
            _displayTimer = new PassiveTimer(_displayDuration);
            _backFillTimer = new PassiveTimer(_backFillDelay);
        }

        private void Start() {
            _canvas = GetComponentInParent<Canvas>();

            Health t = _target;
            _target = null;
            Target = t;
        }

        private void Reset() {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas.renderMode == RenderMode.ScreenSpaceCamera) {
                _trackingCamera = canvas.worldCamera;
            }

            _backImage = GetComponent<Image>();
            Transform fill = transform.GetChild(0);
            if (fill) {
                _fillImage = fill.GetComponent<Image>();
            }
        }

        private void LateUpdate() {
            bool en = Target;
            if (_displayMode == DisplayMode.Hidden) {
                en = false;
            } else if (_displayMode == DisplayMode.OnDamage) {
                en &= !_displayTimer.IsIntervalEnded;
            }

            if (Target) {
                float fillAmount = Target.CurrentHealth / Target.maxHealth;

                if (_fillImage) {
                    if (_fillImage.type == Image.Type.Filled) {
                        _fillImage.fillAmount = fillAmount;
                        _fillImage.rectTransform.anchorMax = Vector2.one;
                    } else {
                        _fillImage.fillAmount = 1.0f;
                        _fillImage.rectTransform.anchorMax = new Vector2(fillAmount, 1);
                    }
                }

                if (_backFillImage) {
                    if (_backFillAmount < fillAmount) {
                        _backFillAmount = fillAmount;
                    } else if (_backFillAmount > fillAmount && _backFillTimer.IsIntervalEnded) {
                        _backFillAmount =
                            Mathf.MoveTowards(_backFillAmount, fillAmount, _backFillSpeed * Time.deltaTime);
                    }

                    if (_backImage.type == Image.Type.Filled) {
                        _backFillImage.fillAmount = _backFillAmount;
                        _backFillImage.rectTransform.anchorMax = Vector2.one;
                    } else {
                        _backFillImage.fillAmount = 1.0f;
                        _backFillImage.rectTransform.anchorMax = new Vector2(_backFillAmount, 1);
                    }
                }

                if (_amountText) {
                    string textValue;
                    if (_amountTextType == AmountTextType.Amount) {
                        textValue = Mathf.CeilToInt(Target.CurrentHealth).ToString();
                    } else if (_amountTextType == AmountTextType.Fraction) {
                        textValue = Mathf.CeilToInt(Target.CurrentHealth) + " / " + Mathf.CeilToInt(Target.maxHealth);
                    } else {
                        textValue = Mathf.CeilToInt(Target.CurrentHealth * 100 / Target.maxHealth).ToString();
                    }

                    if (!string.IsNullOrEmpty(_amountTextFormat)) {
                        _amountText.text = string.Format(_amountTextFormat, textValue);
                    } else {
                        _amountText.text = textValue;
                    }
                }

                Camera cam;
                if (_trackingCamera) {
                    cam = _trackingCamera;
                } else {
                    cam = Camera.main;
                }

                if (_trackOnScreen && cam) {
                    Vector3 wpos = Target.transform.position + _trackWorldOffset;
                    Vector3 spos = cam.WorldToViewportPoint(wpos) + (Vector3) _trackScreenOffset;

                    en &= spos.z > 0;

                    Rect rect = _canvas.pixelRect;
                    spos.z = 0;
                    spos.x *= rect.width;
                    spos.y *= rect.height;
                    spos.x -= rect.width / 2;
                    spos.y -= rect.height / 2;

                    Vector3 s = _canvas.transform.localScale;
                    _canvas.transform.localScale = Vector3.one;
                    Vector3 p = _canvas.transform.TransformPoint(spos);
                    _canvas.transform.localScale = s;
                    transform.position = p;
                }
            }

            _fillImage.enabled = en;
            if (_backImage) _backImage.enabled = en;
            if (_amountText) _amountText.enabled = en;

            foreach (Graphic graphic in _additionalGraphics) {
                graphic.enabled = en;
            }
        }
    }
}
