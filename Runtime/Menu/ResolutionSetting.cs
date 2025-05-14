using System.Linq;
using UnityEngine;

namespace SBR.Menu {
    public class ResolutionSetting : Setting<Resolution> {
        private const string WidthSuffix = "/Width";
        private const string HeightSuffix = "/Height";
        private const string RefreshSuffix = "/Refresh";

        private readonly string _widthKey;
        private readonly string _heightKey;
        private readonly string _refreshKey;

        public ResolutionSetting(string key, string displayName, string description) :
            base(key, displayName, description, Screen.currentResolution, false, Setter, null,
                 Screen.resolutions.Reverse().ToArray()) {
            _widthKey = key + WidthSuffix;
            _heightKey = key + HeightSuffix;
            _refreshKey = key + RefreshSuffix;
        }

        private static void Setter(Resolution value) {
            Screen.SetResolution(value.width, value.height, Screen.fullScreenMode, value.refreshRateRatio);
        }

        public override void Load() {
            Value = new Resolution {
                width = PlayerPrefs.GetInt(_widthKey, DefaultValue.width),
                height = PlayerPrefs.GetInt(_heightKey, DefaultValue.height),
                refreshRate = PlayerPrefs.GetInt(_refreshKey, DefaultValue.refreshRate)
            };
            base.Load();
        }

        public override void Save() {
            PlayerPrefs.SetInt(_widthKey, Value.width);
            PlayerPrefs.SetInt(_heightKey, Value.height);
            PlayerPrefs.SetInt(_refreshKey, Value.refreshRate);
            base.Save();
        }
    }
}
