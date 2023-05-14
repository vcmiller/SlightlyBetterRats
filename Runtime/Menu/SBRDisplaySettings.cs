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

using UnityEngine;

#if SBRGraphicsSettings

namespace SBR.Menu {
    public static class SBRDisplaySettings {
        private const string ScreenResolutionKey = "Display/ScreenResolution";
        private const string FullscreenModeKey = "Display/FullscreenMode";
        private const string VerticalSyncKey = "Display/VerticalSync";

        private const string ScreenResolutionName = "Screen Resolution";
        private const string FullscreenModeName = "Fullscreen Mode";
        private const string VerticalSyncName = "Vertical Sync";

        private const string ScreenResolutionDesc = "Controls the pixel resolution and framerate of the game.";
        private const string FullscreenModeDesc = "Controls whether the game runs in a window.";
        private const string VerticalSyncDesc = "Controls screen buffering.";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#endif
        private static void Register() {
            SettingsManager.RegisterSettings(typeof(SBRDisplaySettings));
        }

#region Settings Definitions

        public static readonly Setting<Resolution> ScreenResolution = 
            new ResolutionSetting(ScreenResolutionKey, ScreenResolutionName, ScreenResolutionDesc);

        public static readonly Setting<FullScreenMode> FullscreenMode =
            new EnumSetting<FullScreenMode>(FullscreenModeKey, FullscreenModeName, FullscreenModeDesc,
                                            FullScreenMode.ExclusiveFullScreen, true, m => Screen.fullScreenMode = m);

        public static readonly Setting<int> VerticalSync =
            new IntSetting(VerticalSyncKey, VerticalSyncName, VerticalSyncDesc, 1, true,
                           v => {
                               QualitySettings.vSyncCount = Mathf.Clamp(v, 0, 1);
                               QualitySettings.maxQueuedFrames = v;
                           },
                           v => {
                               return v switch {
                                   0 => "None",
                                   1 => "Double Buffered",
                                   2 => "Triple Buffered",
                                   _ => null,
                               };
                           },
                           new[] { 0, 1, 2 });

        #endregion
    }
}

#endif