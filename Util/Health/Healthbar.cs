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
        [Conditional("amountText", null, false)]
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
        [Conditional("trackOnScreen")]
        [Tooltip("Camera to use for tracking on screen. Leave empty to use Camera.main.")]
        public Camera trackingCamera;

        /// <summary>
        /// World space offset to apply when tracking the target.
        /// </summary>
        [Conditional("trackOnScreen")]
        [Tooltip("World space offset to apply when tracking the target.")]
        public Vector3 trackWorldOffset = Vector3.up;

        /// <summary>
        /// Screen space offset to apply when tracking the target.
        /// </summary>
        [Conditional("trackOnScreen")]
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
        [Conditional("displayMode", DisplayMode.OnDamage, true)]
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
            displayTimer.Set();
        }
        
        private void Start() {
            fillRect = fillImage.GetComponent<RectTransform>();
            canvas = GetComponentInParent<Canvas>();
            displayTimer = new ExpirationTimer(displayDuration);

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
                    fillImage.fillAmount = target.health / target.maxHealth;
                    fillRect.anchorMax = Vector2.one;
                } else {
                    fillImage.fillAmount = 1.0f;
                    fillRect.anchorMax = new Vector2(target.health / target.maxHealth, 1);
                }
            }

            if (amountText && target) {
                if (amountTextType == AmountTextType.Amount) {
                    amountText.text = Mathf.CeilToInt(target.health).ToString();
                } else if (amountTextType == AmountTextType.Fraction) {
                    amountText.text = Mathf.CeilToInt(target.health) + " / " + Mathf.CeilToInt(target.maxHealth);
                } else {
                    amountText.text = Mathf.CeilToInt(target.health * 100 / target.maxHealth) + " %";
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
            backImage.enabled = en;
            if (amountText) amountText.enabled = en;

            foreach (var graphic in additionalGraphics) {
                graphic.enabled = en;
            }
        }
    }
}