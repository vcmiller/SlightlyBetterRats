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

using System.Linq;
using UnityEngine;

#if SBRGraphicsSettings

namespace SBR.Menu {
    public static class SBRGraphicsSettings {
        private const string AnisotropicKey = "Graphics/Anisotropic";
        private const string AntialiasingKey = "Graphics/Antialiasing";
        private const string BlendWeightsKey = "Graphics/BlendWeights";
        private const string TextureQualityKey = "Graphics/TextureQuality";
        private const string RealtimeReflectionsKey = "Graphics/RealtimeReflections";
        private const string ShadowQualityKey = "Graphics/ShadowQuality";
        private const string ShadowTypeKey = "Graphics/ShadowType";
        private const string SoftParticlesKey = "Graphics/SoftParticles";
        private const string SoftVegetationKey = "Graphics/SoftVegetation";
        private const string OverallQualityLevelKey = "Graphics/OverallQualityLevel";

        private const string AnisotropicName = "Anisotropic Filtering";
        private const string AntialiasingName = "Antialiasing";
        private const string BlendWeightsName = "Blend Weights";
        private const string TextureQualityName = "Texture Quality";
        private const string RealtimeReflectionsName = "Realtime Reflections";
        private const string ShadowQualityName = "Shadow Quality";
        private const string ShadowTypeName = "Shadow Type";
        private const string SoftParticlesName = "Soft Particles";
        private const string SoftVegetationName = "Soft Vegetation";
        private const string OverallQualityLevelName = "Overall Quality Level";

        private const string AnisotropicDesc = "Improves texture quality when viewed at an angle.";
        private const string AntialiasingDesc = "Improves jagged/pixelated edges.";
        private const string BlendWeightsDesc = "Controls quality of character animations.";
        private const string TextureQualityDesc = "Controls quality of textures.";
        private const string RealtimeReflectionsDesc = "Controls whether realtime reflections are shown.";
        private const string ShadowQualityDesc = "Controls the resolution of shadows.";
        private const string ShadowTypeDesc = "Controls whether hard and/or soft shadows are enabled.";
        private const string SoftParticlesDesc = "Makes VFX look better when they intersect with other objects.";
        private const string SoftVegetationDesc = "Improves quality of vegetation rendering.";
        private const string OverallQualityLevelDesc = "Controls overall quality level based on presets.";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#endif
        private static void Register() {
            SettingsManager.RegisterSettings(typeof(SBRGraphicsSettings));
        }

        #region Settings Definitions

        public static readonly Setting<AnisotropicFiltering> Anisotropic =
            new EnumSetting<AnisotropicFiltering>(AnisotropicKey, AnisotropicName, AnisotropicDesc,
                                                  AnisotropicFiltering.Enable, false,
                                                  t => QualitySettings.anisotropicFiltering = t);

        public static readonly Setting<int> Antialiasing =
            new IntSetting(AntialiasingKey, AntialiasingName, AntialiasingDesc, 2, false,
                           t => QualitySettings.antiAliasing = t,
                           t => {
                               if (t == 0) return "None";
                               return t + "x MSAA";
                           }, new[] { 0, 2, 4, 8 });

        public static readonly Setting<SkinWeights> BlendWeights =
            new EnumSetting<SkinWeights>(BlendWeightsKey, BlendWeightsName, BlendWeightsDesc, SkinWeights.FourBones,
                                         false, t => QualitySettings.skinWeights = t);

        public static readonly Setting<int> TextureQuality =
            new IntSetting(TextureQualityKey, TextureQualityName, TextureQualityDesc, 0, false,
                           t => QualitySettings.masterTextureLimit = t,
                           t => {
                               return t switch {
                                   0 => "Full",
                                   1 => "Half",
                                   2 => "Quarter",
                                   3 => "Eighth",
                                   _ => null,
                               };
                           }, new[] { 0, 1, 2, 3 });

        public static readonly Setting<bool> RealtimeReflections =
            new BoolSetting(RealtimeReflectionsKey, RealtimeReflectionsName, RealtimeReflectionsDesc, true, false,
                            t => QualitySettings.realtimeReflectionProbes = t);

        public static readonly Setting<ShadowQuality> ShadowType =
            new EnumSetting<ShadowQuality>(ShadowTypeKey, ShadowTypeName, ShadowTypeDesc, UnityEngine.ShadowQuality.All,
                                           false, t => QualitySettings.shadows = t);

        public static readonly Setting<ShadowResolution> ShadowQuality =
            new EnumSetting<ShadowResolution>(ShadowQualityKey, ShadowQualityName, ShadowQualityDesc,
                                              ShadowResolution.High, false, t => QualitySettings.shadowResolution = t);

        public static readonly Setting<bool> SoftParticles =
            new BoolSetting(SoftParticlesKey, SoftParticlesName, SoftParticlesDesc, true, false,
                            t => QualitySettings.softParticles = t);

        public static readonly Setting<bool> SoftVegetation =
            new BoolSetting(SoftVegetationKey, SoftVegetationName, SoftVegetationDesc, true, false,
                            t => QualitySettings.softVegetation = t);

        public static readonly Setting<int> OverallQualityLevel =
            new IntSetting(OverallQualityLevelKey, OverallQualityLevelName, OverallQualityLevelDesc,
                           QualitySettings.names.Length / 2, true, t => {
                               if (t < 0) return;
                               QualitySettings.SetQualityLevel(t);
                               Anisotropic.Value = QualitySettings.anisotropicFiltering;
                               Antialiasing.Value = QualitySettings.antiAliasing;
                               BlendWeights.Value = QualitySettings.skinWeights;
                               TextureQuality.Value = QualitySettings.masterTextureLimit;
                               RealtimeReflections.Value = QualitySettings.realtimeReflectionProbes;
                               ShadowType.Value = QualitySettings.shadows;
                               ShadowQuality.Value = QualitySettings.shadowResolution;
                               SoftParticles.Value = QualitySettings.softParticles;
                               SoftVegetation.Value = QualitySettings.softVegetation;
                               SBRDisplaySettings.VerticalSync.Value = QualitySettings.maxQueuedFrames;
                           }, t => {
                               if (t < 0) return "Custom";
                               else return QualitySettings.names[t];
                           }, Enumerable.Range(-1, QualitySettings.names.Length + 1).ToArray());

        #endregion
    }
}

#endif