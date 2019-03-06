using UnityEngine;
using UnityEngine.UI;

namespace SBR.Menu {
    public class TabList : MonoBehaviour {
        public Button buttonPrefab;
        public ShowHideUI[] tabs;
        public int currentTab;

        public string nextButton;
        public string prevButton;

        public Color onLabelColor = Color.white;
        public Color offLabelColor = Color.white;

        private Image[] tabImages;
        private Text[] tabLabels;

        private void UpdateActiveTab(int tab) {
            currentTab = tab;
            for (int i = 0; i < tabs.Length; i++) {
                tabs[i].show = i == currentTab;
                tabImages[i].enabled = i == currentTab;
                tabLabels[i].color = i == currentTab ? onLabelColor : offLabelColor;
            }
        }

        private void Awake() {
            tabImages = new Image[transform.childCount];
            tabLabels = new Text[transform.childCount];
            for (int i = 0; i < transform.childCount; i++) {
                int cpy = i;
                var b = transform.GetChild(i).GetComponent<Button>();
                b.onClick.AddListener(() => UpdateActiveTab(cpy));
                tabImages[i] = transform.GetChild(i).Find("Fill").GetComponent<Image>();
                tabLabels[i] = transform.GetChild(i).GetComponentInChildren<Text>();
            }
            UpdateActiveTab(currentTab);
        }

        private void Update() {
            if (!string.IsNullOrEmpty(nextButton) && Input.GetButtonDown(nextButton)) {
                UpdateActiveTab((currentTab + 1) % tabs.Length);
            } else if (!string.IsNullOrEmpty(prevButton) && Input.GetButtonDown(prevButton)) {
                UpdateActiveTab((currentTab - 1 + tabs.Length) % tabs.Length);
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
                var b = (Button)UnityEditor.PrefabUtility.InstantiatePrefab(buttonPrefab);
                var t = b.GetComponentInChildren<Text>();
                t.text = tab.name;
                b.transform.SetParent(transform, false);
            }
        }
#endif
    }

}