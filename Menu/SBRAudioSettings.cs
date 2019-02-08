using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
            new FloatSetting(masterVolumeKey, 1.0f, values: new[] { 0.0f, 1.0f });

        public static readonly Setting<float> effectsVolume =
            new FloatSetting(effectsVolumeKey, 1.0f, values: new[] { 0.0f, 1.0f });

        public static readonly Setting<float> musicVolume =
            new FloatSetting(musicVolumeKey, 1.0f, values: new[] { 0.0f, 1.0f });

        public static readonly Setting<float> voiceVolume =
            new FloatSetting(voiceVolumeKey, 1.0f, values: new[] { 0.0f, 1.0f });

        #endregion
    }

}