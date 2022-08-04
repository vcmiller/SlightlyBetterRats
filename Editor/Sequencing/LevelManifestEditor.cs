using System.Collections;
using System.Collections.Generic;
using Infohazard.Core.Editor;
using SBR.Sequencing;

using UnityEditor;

using UnityEngine;

namespace SBR.Editor.Sequencing {
    [CustomEditor(typeof(LevelManifest))]
    public class LevelManifestEditor : UnityEditor.Editor {
        public override void OnInspectorGUI() {
            var action = ExpandableAttributeDrawer.SaveAction;
            ExpandableAttributeDrawer.SaveAction = SaveAction;

            DrawDefaultInspector();

            ExpandableAttributeDrawer.SaveAction = action;
            
            SetRegionLevelIDs();
            ClearDuplicateEntries();

            if (GUILayout.Button("Cleanup Unused Objects")) {
                CleanupUnusedObjects();
            }
        }

        private void SetRegionLevelIDs() {
            var manifest = (LevelManifest) target;
            if (manifest.Levels == null) return;

            foreach (LevelManifestLevelEntry level in manifest.Levels) {
                if (!level || level.Regions == null) continue;
                foreach (LevelManifestRegionEntry region in level.Regions) {
                    if (!region || region.LevelID == level.LevelID) continue;

                    SerializedObject so = new SerializedObject(region);
                    SerializedProperty prop = so.FindProperty("_levelID");
                    prop.intValue = level.LevelID;
                    so.ApplyModifiedProperties();
                }
            }
        }

        private void ClearDuplicateEntries() {
            var manifest = (LevelManifest) target;
            if (manifest.Levels == null) return;

            HashSet<LevelManifestLevelEntry> seenLevels = new HashSet<LevelManifestLevelEntry>();
            HashSet<LevelManifestRegionEntry> seenRegions = new HashSet<LevelManifestRegionEntry>();

            serializedObject.Update();
            SerializedProperty levelsProp = serializedObject.FindProperty("_levels");

            for (int levelIndex = 0; levelIndex < manifest.Levels.Count; levelIndex++) {
                LevelManifestLevelEntry level = manifest.Levels[levelIndex];
                if (!level) continue;
                if (!seenLevels.Add(level)) {
                    levelsProp.GetArrayElementAtIndex(levelIndex).objectReferenceValue = null;
                    continue;
                }
                
                if (level.Regions == null) continue;
                SerializedObject so = new SerializedObject(level);
                SerializedProperty regionsProperty = so.FindProperty("_regions");
                for (int regionIndex = 0; regionIndex < level.Regions.Count; regionIndex++) {
                    LevelManifestRegionEntry region = level.Regions[regionIndex];
                    if (!region || seenRegions.Add(region)) continue;

                    regionsProperty.GetArrayElementAtIndex(regionIndex).objectReferenceValue = null;
                }

                so.ApplyModifiedProperties();
            }

            serializedObject.ApplyModifiedProperties();
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
                    Undo.DestroyObjectImmediate(childAsset);
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