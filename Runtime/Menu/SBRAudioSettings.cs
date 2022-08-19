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

#if SBRAudioSettings

namespace SBR.Menu {
    public static class SBRAudioSettings {
        private const string MasterVolumeKey = "Audio/MasterVolume";
        private const string EffectsVolumeKey = "Audio/EffectsVolume";
        private const string MusicVolumeKey = "Audio/MusicVolume";
        private const string VoiceVolumeKey = "Audio/VoiceVolume";

        private const string MasterVolumeName = "Master Volume";
        private const string EffectsVolumeName = "Effects Volume";
        private const string MusicVolumeName = "Music Volume";
        private const string VoiceVolumeName = "Voice Volume";

        private const string MasterVolumeDesc = "Controls the volume of all sounds and music.";
        private const string EffectsVolumeDesc = "Controls the volume of sound effects.";
        private const string MusicVolumeDesc = "Controls the volume of music.";
        private const string VoiceVolumeDesc = "Controls the volume of character voices.";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#endif
        private static void Register() {
            SettingsManager.RegisterSettings(typeof(SBRAudioSettings));
        }

        #region Settings Definitions

        public static readonly Setting<float> MasterVolume =
            new FloatSetting(MasterVolumeKey, MasterVolumeName, MasterVolumeDesc, 1.0f, true,
                v => AudioListener.volume = v,
                values: new[] { 0.0f, 1.0f });

        public static readonly Setting<float> EffectsVolume =
            new FloatSetting(EffectsVolumeKey, EffectsVolumeName, EffectsVolumeDesc, 1.0f, values: new[] { 0.0f, 1.0f });

        public static readonly Setting<float> MusicVolume =
            new FloatSetting(MusicVolumeKey, MusicVolumeName, MusicVolumeDesc, 1.0f, values: new[] { 0.0f, 1.0f });

        public static readonly Setting<float> VoiceVolume =
            new FloatSetting(VoiceVolumeKey, VoiceVolumeName, VoiceVolumeDesc, 1.0f, values: new[] { 0.0f, 1.0f });

#endregion
    }
}

#endif