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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if SBRAudioSettings

namespace SBR.Menu {
    public static class SBRAudioSettings {
        private const string masterVolumeKey = "Audio/MasterVolume";
        private const string effectsVolumeKey = "Audio/EffectsVolume";
        private const string musicVolumeKey = "Audio/MusicVolume";
        private const string voiceVolumeKey = "Audio/VoiceVolume";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#endif
        private static void Register() {
            SettingsManager.RegisterSettings(typeof(SBRAudioSettings));
        }

        #region Settings Definitions

        public static readonly Setting<float> masterVolume =
            new FloatSetting(masterVolumeKey, 1.0f, true,
                v => AudioListener.volume = v,
                values: new[] { 0.0f, 1.0f });

        public static readonly Setting<float> effectsVolume =
            new FloatSetting(effectsVolumeKey, 1.0f, values: new[] { 0.0f, 1.0f });

        public static readonly Setting<float> musicVolume =
            new FloatSetting(musicVolumeKey, 1.0f, values: new[] { 0.0f, 1.0f });

        public static readonly Setting<float> voiceVolume =
            new FloatSetting(voiceVolumeKey, 1.0f, values: new[] { 0.0f, 1.0f });

#endregion
    }
}

#endif