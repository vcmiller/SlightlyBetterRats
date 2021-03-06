﻿// MIT License
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

using UnityEngine;

#if SBRGraphicsSettings

namespace SBR.Menu {
    public static class SBRDisplaySettings {
        private const string resolutionKey = "Display/ScreenResolution";
        private const string fullscreenKey = "Display/FullscreenMode";
        private const string vsyncKey = "Display/VerticalSync";

        private const string widthSuffix = "/Width";
        private const string heightSuffix = "/Height";
        private const string refreshSuffix = "/Refresh";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#endif
        private static void Register() {
            SettingsManager.RegisterSettings(typeof(SBRDisplaySettings));
        }

#region Settings Types

        private class ResolutionSetting : Setting<Resolution> {
            private string widthKey, heightKey, refreshKey;

            public ResolutionSetting(string key) : 
                base(key, Screen.currentResolution, false, Setter, null, Screen.resolutions) {
                widthKey = key + widthSuffix;
                heightKey = key + heightSuffix;
                refreshKey = key + refreshSuffix;
            }

            private static void Setter(Resolution value) {
                Screen.SetResolution(value.width, value.height, fullscreenMode.value, value.refreshRate);
            }

            public override void Load() {
                value = new Resolution() {
                    width = PlayerPrefs.GetInt(widthKey, defaultValue.width),
                    height = PlayerPrefs.GetInt(heightKey, defaultValue.height),
                    refreshRate = PlayerPrefs.GetInt(refreshKey, defaultValue.refreshRate)
                };
                base.Load();
            }

            public override void Save() {
                PlayerPrefs.SetInt(widthKey, value.width);
                PlayerPrefs.SetInt(heightKey, value.height);
                PlayerPrefs.SetInt(refreshKey, value.refreshRate);
                base.Save();
            }
        }

#endregion

#region Settings Definitions

        public static readonly Setting<Resolution> resolution = 
            new ResolutionSetting(resolutionKey);

        public static readonly Setting<FullScreenMode> fullscreenMode =
            new EnumSetting<FullScreenMode>(fullscreenKey, 
                FullScreenMode.ExclusiveFullScreen,
                true,
                m => Screen.fullScreenMode = m);

        public static readonly Setting<int> vsyncCount =
            new IntSetting(vsyncKey,
                1, true,
                v => {
                    QualitySettings.vSyncCount = Mathf.Clamp(v, 0, 1);
                    QualitySettings.maxQueuedFrames = v;
                },
                v => {
                    switch (v) {
                        case 0: return "None";
                        case 1: return "Double Buffered";
                        case 2: return "Triple Buffered";
                        default: return null;
                    }
                },
                new[] { 0, 1, 2 });

#endregion
    }
}

#endif