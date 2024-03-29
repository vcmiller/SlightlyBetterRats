﻿// The MIT License (MIT)
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

using Infohazard.Core.Editor;
using UnityEngine;
using UnityEditor;

namespace SBR.Editor {
    public class SBRProjectSettings : ScriptableObject {
        

        [InitializeOnLoadMethod]
        private static void Register() {
            // Force instance to be created immediately.
            var temp = inst;
        }

        [MenuItem("Tools/Infohazard/Select/SBR Settings...")]
        public static void SelectSettings() {
            Selection.activeObject = inst;
        }


        private static SBRProjectSettings _inst;
        public static SBRProjectSettings inst {
            get {
                if (_inst) return _inst;
                string path = CoreEditorUtility.DataFolder + "SBRProjectSettings.asset";
                _inst = AssetDatabase.LoadAssetAtPath<SBRProjectSettings>(path);
                if (_inst) return _inst;
                _inst = CreateInstance<SBRProjectSettings>();
                CoreEditorUtility.EnsureDataFolderExists();
                AssetDatabase.CreateAsset(_inst, path);
                AssetDatabase.SaveAssets();
                return _inst;
            }
        }

        private bool _includeAudioSettings;
        private bool _includeGraphicsSettings;

        public bool includeAudioSettings;
        public bool includeGraphicsSettings;
        public bool autoLoadScene0InEditor;

        private void OnValidate() {
            if (includeAudioSettings != _includeAudioSettings) {
                _includeAudioSettings = includeAudioSettings;
                SBREditorUtility.SetSymbolDefined("SBRAudioSettings", includeAudioSettings);
            }
            if (includeGraphicsSettings != _includeGraphicsSettings) {
                _includeGraphicsSettings = includeGraphicsSettings;
                SBREditorUtility.SetSymbolDefined("SBRGraphicsSettings", includeGraphicsSettings);
            }
        }
    }

}