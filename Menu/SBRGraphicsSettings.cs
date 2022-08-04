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

#if SBRGraphicsSettings

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
                AnisotropicFiltering.Enable, false,
                t => QualitySettings.anisotropicFiltering = t, null);

        public static readonly Setting<int> antialiasing =
            new IntSetting(antialiasingKey, 2, false,
                t => QualitySettings.antiAliasing = t,
                t => {
                    if (t == 0) return "None";
                    else return t + "x MSAA";
                }, new[] { 0, 2, 4, 8 });

        public static readonly Setting<SkinWeights> blendWeights =
            new EnumSetting<SkinWeights>(blendWeightsKey, SkinWeights.FourBones, false,
                t => QualitySettings.skinWeights = t);

        public static readonly Setting<int> textureQuality =
            new IntSetting(textureQualityKey, 0, false,
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
            new BoolSetting(realtimeReflectionsKey, true, false,
                t => QualitySettings.realtimeReflectionProbes = t);

        public static readonly Setting<ShadowQuality> shadowType =
            new EnumSetting<ShadowQuality>(shadowTypeKey,
                ShadowQuality.All, false,
                t => QualitySettings.shadows = t);

        public static readonly Setting<ShadowResolution> shadowQuality =
            new EnumSetting<ShadowResolution>(shadowQualityKey,
                ShadowResolution.High, false,
                t => QualitySettings.shadowResolution = t);

        public static readonly Setting<bool> softParticles =
            new BoolSetting(softParticlesKey, true, false,
                t => QualitySettings.softParticles = t);

        public static readonly Setting<bool> softVegetation =
            new BoolSetting(softVegetationKey, true, false,
                t => QualitySettings.softVegetation = t);

        public static readonly Setting<int> overallQualityLevel =
            new IntSetting(overallQualityLevelKey,
                QualitySettings.names.Length / 2, true, t => {
                    if (t >= 0) {
                        QualitySettings.SetQualityLevel(t);
                        anisotropic.value = QualitySettings.anisotropicFiltering;
                        antialiasing.value = QualitySettings.antiAliasing;
                        blendWeights.value = QualitySettings.skinWeights;
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

#endif