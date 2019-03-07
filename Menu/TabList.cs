using UnityEngine;
using UnityEngine.UI;

namespace SBR.Menu {
    public class TabList : MonoBehaviour {
        public TabButton buttonPrefab;
        public ShowHideUI[] tabs;
        public int currentTab;

        public string scrollAxis;
        public float repeatDelay = 0.5f;

        private TabButton[] buttons;
        private CooldownTimer scrollTimer;

        private void UpdateActiveTab(int tab) {
            currentTab = tab;
            for (int i = 0; i < tabs.Length; i++) {
                tabs[i].show = i == currentTab;
                buttons[i].isOn = i == currentTab;
            }
        }

        private void Awake() {
            scrollTimer = new CooldownTimer(repeatDelay);
            buttons = new TabButton[transform.childCount];
            for (int i = 0; i < transform.childCount; i++) {
                int cpy = i;
                buttons[i] = transform.GetChild(i).GetComponent<TabButton>();
                buttons[i].button.onClick.AddListener(() => UpdateActiveTab(cpy));
            }
            UpdateActiveTab(currentTab);
        }

        private void Update() {
            if (!string.IsNullOrEmpty(scrollAxis)) {
                float f = Input.GetAxis(scrollAxis);
                if (f != 0 && scrollTimer.Use()) {
                    int newTab = currentTab + (int)Mathf.Sign(f);
                    newTab = (newTab + tabs.Length) % tabs.Length;
                    UpdateActiveTab(newTab);
                } else if (f == 0) {
                    scrollTimer.Clear();
                }
            }
        }

#if UNITY_EDITOR
        public void ClearUI() {
            while (transform.childCount > 0) {
                DestroyImmediate(transform.GetChild(0).gameObject);
            }
        }

        public void UpdateUI() {
            ClearUI();

            for (int i = 0; i < tabs.Length; i++) {
                var tab = tabs[i];
                var b = (TabButton)UnityEditor.PrefabUtility.InstantiatePrefab(buttonPrefab);
                if (b.label) b.label.text = tab.name;
                b.transform.SetParent(transform, false);
            }
        }
#endif
    }

}