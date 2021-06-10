using System.Collections;
using System.Collections.Generic;

using SBR.Startup;

using UnityEditor;

using UnityEngine;

namespace SBR.Editor.Startup {
    [CustomEditor(typeof(LevelManifest))]
    public class LevelManifestEditor : UnityEditor.Editor {
        public override void OnInspectorGUI() {
            var action = ExpandableAttributeDrawer.SaveAction;
            ExpandableAttributeDrawer.SaveAction = SaveAction;

            DrawDefaultInspector();

            ExpandableAttributeDrawer.SaveAction = action;
            
            CleanupUnusedObjects();
        }

        private static HashSet<Object> _validChildObjects = new HashSet<Object>();
        private void CleanupUnusedObjects() {
            _validChildObjects.Clear();
            var manifest = (LevelManifest) target;
            if (manifest.Levels == null) return;
            
            foreach (LevelManifestLevelEntry level in manifest.Levels) {
                if (!level) continue;
                _validChildObjects.Add(level);
                if (level.Regions == null) continue;
                foreach (LevelManifestRegionEntry region in level.Regions) {
                    if (!region) continue;
                    _validChildObjects.Add(region);
                }
            }

            bool modified = false;
            string path = AssetDatabase.GetAssetPath(target);
            if (!string.IsNullOrEmpty(path)) {
                foreach (Object childAsset in AssetDatabase.LoadAllAssetsAtPath(path)) {
                    if (childAsset == target || _validChildObjects.Contains(childAsset)) continue;
                    DestroyImmediate(childAsset, true);
                    modified = true;
                }

                if (modified) {
                    AssetDatabase.SaveAssets();
                    AssetDatabase.ImportAsset(path);
                }
            }
        }

        private void SaveAction(ScriptableObject asset, string path) {
            AssetDatabase.AddObjectToAsset(asset, target);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(asset));
        }
    }
}