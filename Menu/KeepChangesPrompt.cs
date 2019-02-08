using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SBR.Menu {
    public class KeepChangesPrompt : MonoBehaviour {
        public float time;
        private string textTemplate;
        public Text text;
        public SettingsSubMenu settings;

        private ExpirationTimer timer;

        private void Awake() {
            textTemplate = text.text;
        }

        private void OnEnable() {
            timer = new ExpirationTimer(time);
            timer.Set();
        }

        private void Update() {
            if (text) text.text = string.Format(textTemplate, Mathf.CeilToInt(timer.remaining));
            if (timer.expiredThisFrame) {
                Revert();
            }
        }

        public void Revert() {
            settings.Revert();
            gameObject.SetActive(false);
        }

        public void Confirm() {
            settings.Save();
            gameObject.SetActive(false);
        }
    }
}