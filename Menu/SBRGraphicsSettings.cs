﻿using System.Linq;
using UnityEngine;

namespace SBR.Menu {
    public static class SBRGraphicsSettings {
        private const string anisotropicKey = "Graphics/Anisotropic";
        private const string antialiasingKey = "Graphics/Antialiasing";
        private const string blendWeightsKey = "Graphics/BlendWeights";
        private const string textureQualityKey = "Graphics/TextureQuality";
        private const string realtimeReflectionsKey = "Graphics/RealtimeReflections";
        private const string shadowQualityKey = "Graphics/ShadowQuality";
        private const string shadowTypeKey = "Graphics/ShadowType";
        private const string softParticlesKey = "Graphics/SoftParticles";
        private const string softVegetationKey = "Graphics/SoftVegetation";
        private const string overallQualityLevelKey = "Graphics/OverallQualityLevel";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#endif
        private static void Register() {
            SettingsManager.RegisterSettings(typeof(SBRGraphicsSettings));
        }

        #region Settings Definitions

        public static readonly Setting<AnisotropicFiltering> anisotropic =
            new EnumSetting<AnisotropicFiltering>(anisotropicKey,
                AnisotropicFiltering.Enable,
                t => QualitySettings.anisotropicFiltering = t, null);

        public static readonly Setting<int> antialiasing =
            new IntSetting(antialiasingKey, 2,
                t => QualitySettings.antiAliasing = t,
                t => {
                    if (t == 0) return "None";
                    else return t + "x MSAA";
                }, new[] { 0, 2, 4, 8 });

        public static readonly Setting<BlendWeights> blendWeights =
            new EnumSetting<BlendWeights>(blendWeightsKey, BlendWeights.FourBones,
                t => QualitySettings.blendWeights = t);

        public static readonly Setting<int> textureQuality =
            new IntSetting(textureQualityKey, 0,
                t => QualitySettings.masterTextureLimit = t,
                t => {
                    switch(t) {
                        case 0: return "Full";
                        case 1: return "Half";
                        case 2: return "Quarter";
                        case 3: return "Eighth";
                        default: return null;
                    }
                }, new[] { 0, 1, 2, 3 });

        public static readonly Setting<bool> realtimeReflections =
            new BoolSetting(realtimeReflectionsKey, true,
                t => QualitySettings.realtimeReflectionProbes = t);

        public static readonly Setting<ShadowQuality> shadowType =
            new EnumSetting<ShadowQuality>(shadowTypeKey,
                ShadowQuality.All,
                t => QualitySettings.shadows = t);

        public static readonly Setting<ShadowResolution> shadowQuality =
            new EnumSetting<ShadowResolution>(shadowQualityKey,
                ShadowResolution.High,
                t => QualitySettings.shadowResolution = t);

        public static readonly Setting<bool> softParticles =
            new BoolSetting(softParticlesKey, true,
                t => QualitySettings.softParticles = t);

        public static readonly Setting<bool> softVegetation =
            new BoolSetting(softVegetationKey, true,
                t => QualitySettings.softVegetation = t);

        public static readonly Setting<int> overallQualityLevel =
            new IntSetting(overallQualityLevelKey,
                QualitySettings.names.Length - 1, t => {
                    if (t >= 0) {
                        QualitySettings.SetQualityLevel(t);
                        anisotropic.value = QualitySettings.anisotropicFiltering;
                        antialiasing.value = QualitySettings.antiAliasing;
                        blendWeights.value = QualitySettings.blendWeights;
                        textureQuality.value = QualitySettings.masterTextureLimit;
                        realtimeReflections.value = QualitySettings.realtimeReflectionProbes;
                        shadowType.value = QualitySettings.shadows;
                        shadowQuality.value = QualitySettings.shadowResolution;
                        softParticles.value = QualitySettings.softParticles;
                        softVegetation.value = QualitySettings.softVegetation;
                        SBRDisplaySettings.vsyncCount.value = QualitySettings.maxQueuedFrames;
                    }
                }, t => {
                    if (t < 0) return "Custom";
                    else return QualitySettings.names[t];
                }, Enumerable.Range(-1, QualitySettings.names.Length + 1).ToArray());

        #endregion
    }
}