// MIT License
// 
// Copyright (c) 2020 Vincent Miller
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
using Infohazard.Core.Runtime;
using UnityEngine;
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
        [Tooltip("Image that is used for the bar itself.")]
        public Image fillImage;

        /// <summary>
        /// Image that is used for the background of the healthbar.
        /// </summary>
        [Tooltip("Image that is used for the background of the healthbar.")]
        public Image backImage;

        /// <summary>
        /// Text used to show amount.
        /// </summary>
        [Tooltip("Text used to show amount.")]
        public Text amountText;

        /// <summary>
        /// How to display the amount text.
        /// </summary>
        [Tooltip("How to display the amount text.")]
        [ConditionalDraw("amountText", null, false)]
        public AmountTextType amountTextType;

        /// <summary>
        /// Optional additional graphics which will be shown and hidden along with the rest of the Healthbar.
        /// </summary>
        [Tooltip("Optional additional graphics which will be shown and hidden along with the rest of the Healthbar.")]
        public Graphic[] additionalGraphics;

        /// <summary>
        /// Whether to track the location of the target on screen.
        /// </summary>
        [Tooltip("Whether to track the location of the target on screen.")]
        public bool trackOnScreen;

        /// <summary>
        /// Camera to use for tracking on screen. Leave empty to use Camera.main.
        /// </summary>
        [ConditionalDraw("trackOnScreen")]
        [Tooltip("Camera to use for tracking on screen. Leave empty to use Camera.main.")]
        public Camera trackingCamera;

        /// <summary>
        /// World space offset to apply when tracking the target.
        /// </summary>
        [ConditionalDraw("trackOnScreen")]
        [Tooltip("World space offset to apply when tracking the target.")]
        public Vector3 trackWorldOffset = Vector3.up;

        /// <summary>
        /// Screen space offset to apply when tracking the target.
        /// </summary>
        [ConditionalDraw("trackOnScreen")]
        [Tooltip("Screen space offset to apply when tracking the target.")]
        public Vector2 trackScreenOffset;

        /// <summary>
        /// How the Healthbar is displayed.
        /// </summary>
        [Tooltip("How the Healthbar is displayed.")]
        public DisplayMode displayMode = DisplayMode.Visible;

        /// <summary>
        /// When using DisplayMode.OnDamage, how long to show the Healthbar when it is damaged.
        /// </summary>
        [ConditionalDraw("displayMode", DisplayMode.OnDamage, true)]
        [Tooltip("How long to show the Healthbar when it is damaged.")]
        public float displayDuration = 3;
        
        /// <summary>
        /// Target Health that this Healthbar monitors.
        /// </summary>
        public Health target {
            get { return _target; }
            set {
                if (_target != value) {
                    if (_target) {
                        _target.Damaged -= HealthDamaged;
                    }

                    _target = value;

                    if (_target) {
                        _target.Damaged += HealthDamaged;
                    }
                }
            }
        }

        public enum DisplayMode {
            Hidden, Visible, OnDamage
        }

        public enum AmountTextType {
            Percent, Fraction, Amount
        }

        private RectTransform fillRect;
        private Canvas canvas;
        private ExpirationTimer displayTimer;

        private void HealthDamaged(Damage dmg) {
            displayTimer?.Set();
        }

        private void Awake() {
            displayTimer = new ExpirationTimer(displayDuration);
        }

        private void Start() {
            fillRect = fillImage.GetComponent<RectTransform>();
            canvas = GetComponentInParent<Canvas>();

            var t = _target;
            _target = null;
            target = t;
        }

        private void Reset() {
            var canvas = GetComponentInParent<Canvas>();
            if (canvas.renderMode == RenderMode.ScreenSpaceCamera) {
                trackingCamera = canvas.worldCamera;
            }

            backImage = GetComponent<Image>();
            var fill = transform.GetChild(0);
            if (fill) {
                fillImage = fill.GetComponent<Image>();
            }
        }
        
        private void LateUpdate() {
            bool en = target;
            if (displayMode == DisplayMode.Hidden) {
                en = false;
            } else if (displayMode == DisplayMode.OnDamage) {
                en &= !displayTimer.expired;
            }

            if (fillImage && target) {
                if (fillImage.type == Image.Type.Filled) {
                    fillImage.fillAmount = target.CurrentHealth / target.maxHealth;
                    fillRect.anchorMax = Vector2.one;
                } else {
                    fillImage.fillAmount = 1.0f;
                    fillRect.anchorMax = new Vector2(target.CurrentHealth / target.maxHealth, 1);
                }
            }

            if (amountText && target) {
                if (amountTextType == AmountTextType.Amount) {
                    amountText.text = Mathf.CeilToInt(target.CurrentHealth).ToString();
                } else if (amountTextType == AmountTextType.Fraction) {
                    amountText.text = Mathf.CeilToInt(target.CurrentHealth) + " / " + Mathf.CeilToInt(target.maxHealth);
                } else {
                    amountText.text = Mathf.CeilToInt(target.CurrentHealth * 100 / target.maxHealth) + " %";
                }
            }

            Camera cam;
            if (trackingCamera) {
                cam = trackingCamera;
            } else {
                cam = Camera.main;
            }

            if (target && trackOnScreen && cam) {
                Vector3 wpos = target.transform.position + trackWorldOffset;
                Vector3 spos = cam.WorldToViewportPoint(wpos) + (Vector3)trackScreenOffset;
                
                en &= spos.z > 0;

                var rect = canvas.pixelRect;
                spos.z = 0;
                spos.x *= rect.width;
                spos.y *= rect.height;
                spos.x -= rect.width / 2;
                spos.y -= rect.height / 2;

                var s = canvas.transform.localScale;
                canvas.transform.localScale = Vector3.one;
                Vector3 p = canvas.transform.TransformPoint(spos);
                canvas.transform.localScale = s;
                transform.position = p;
            }

            fillImage.enabled = en;
            if (backImage) backImage.enabled = en;
            if (amountText) amountText.enabled = en;

            foreach (var graphic in additionalGraphics) {
                graphic.enabled = en;
            }
        }
    }
}