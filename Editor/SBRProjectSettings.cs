using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace SBR.Editor {
    public class SBRProjectSettings : ScriptableObject {
        private const string dataAsmRefContent = @"
{
    ""reference"": ""SlightlyBetterRats""
}
";
        private const string dataAsmRefFile = "SBR_Data.asmref";

        [InitializeOnLoadMethod]
        private static void Register() {
            // Force instance to be created immediately.
            var temp = inst;
        }

        [MenuItem("Edit/SBR Settings...")]
        public static void SelectSettings() {
            Selection.activeObject = inst;
        }

        public const string dataFolder = "Assets/SBR_Data/";

        private static SBRProjectSettings _inst;
        public static SBRProjectSettings inst {
            get {
                if (_inst) return _inst;
                string path = dataFolder + "SBRProjectSettings.asset";
                _inst = AssetDatabase.LoadAssetAtPath<SBRProjectSettings>(path);
                if (_inst) return _inst;
                _inst = CreateInstance<SBRProjectSettings>();
                EnsureDataFolderExists();
                AssetDatabase.CreateAsset(_inst, path);
                AssetDatabase.SaveAssets();
                return _inst;
            }
        }

        private bool _includeAudioSettings;
        private bool _includeGraphicsSettings;

        public bool includeAudioSettings;
        public bool includeGraphicsSettings;

        public static void EnsureDataFolderExists() {
            if (!Directory.Exists(dataFolder)) {
                Directory.CreateDirectory(dataFolder);
            }
            string fullAsmRefPath = Path.Combine(dataFolder, dataAsmRefFile);
            if (!File.Exists(fullAsmRefPath)) {
                File.WriteAllText(fullAsmRefPath, dataAsmRefContent);
            }
        }

        private void OnValidate() {
            if (includeAudioSettings != _includeAudioSettings) {
                _includeAudioSettings = includeAudioSettings;
                EditorUtil.SetSymbolDefined("SBRAudioSettings", includeAudioSettings);
            }
            if (includeGraphicsSettings != _includeGraphicsSettings) {
                _includeGraphicsSettings = includeGraphicsSettings;
                EditorUtil.SetSymbolDefined("SBRGraphicsSettings", includeGraphicsSettings);
            }
        }
    }

}