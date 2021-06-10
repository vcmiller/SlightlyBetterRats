using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SBR.Startup {
    public class LevelManifest : SingletonAsset<LevelManifest> {
        [SerializeField] [Expandable(false, typeof(LevelManifestLevelEntry))]
        private LevelManifestLevelEntry[] _levels;

        public override string ResourceFolderPath => "SBR_Data/Resources";
        public override string ResourcePath => "LevelManifest.asset";
        
        public IReadOnlyList<LevelManifestLevelEntry> Levels => _levels;
        
#if UNITY_EDITOR
        [MenuItem("Assets/Level Manifest")]
        public static void SelectLevelManifest() {
            Selection.activeObject = Instance;
        }
#endif
    }
}