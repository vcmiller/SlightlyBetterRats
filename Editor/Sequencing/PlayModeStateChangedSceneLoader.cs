using System.Collections.Generic;

using SBR.Sequencing;

using UnityEditor;
using UnityEditor.SceneManagement;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace SBR.Editor.Sequencing {
    [InitializeOnLoad]
    public static class PlayModeStateChangedSceneLoader {
        static PlayModeStateChangedSceneLoader() {
            if (!SBRProjectSettings.inst.autoLoadScene0InEditor) return;
            EditorApplication.playModeStateChanged -= EditorApplication_PlayModeStateChanged;
            EditorApplication.playModeStateChanged += EditorApplication_PlayModeStateChanged;
        }

        private static void EditorApplication_PlayModeStateChanged(PlayModeStateChange mode) {
            if (!SBRProjectSettings.inst.autoLoadScene0InEditor) return;
            if (mode == PlayModeStateChange.ExitingEditMode) {
                LoadFromInitialScene();
            } else if (mode == PlayModeStateChange.EnteredEditMode) {
                ReloadEditingScenes();
            }
        }

        public static void LoadFromInitialScene() {
            EditorSceneManager.SaveOpenScenes();
            EditorPrefs.SetString("ActiveScene", SceneManager.GetActiveScene().path);
            EditorPrefs.SetInt("OpenSceneCount", SceneManager.sceneCount);
            for (int i = 0; i < SceneManager.sceneCount; i++) {
                var scene = SceneManager.GetSceneAt(i);
                EditorPrefs.SetString($"OpenScenes[{i}].name", scene.path);
                EditorPrefs.SetBool($"OpenScenes[{i}].loaded", scene.isLoaded);
            }

            EditorPrefs.SetInt("OpenLevel", -1);
            foreach (var level in LevelManifest.Instance.Levels) {
                bool isLoaded = SceneManager.GetSceneByPath(level.Scene.Path).isLoaded;
                foreach (LevelManifestRegionEntry region in level.Regions) {
                    bool isRegionLoaded = SceneManager.GetSceneByPath(region.Scene.Path).isLoaded;
                    EditorPrefs.SetBool($"OpenLevelRegions[{region.RegionID}].loaded", isRegionLoaded);
                    if (isRegionLoaded) isLoaded = true;
                }

                if (isLoaded) {
                    EditorPrefs.SetInt("OpenLevel", level.LevelID);
                    break;
                }
            }

            EditorSceneManager.OpenScene(EditorBuildSettings.scenes[0].path, OpenSceneMode.Single);
        }

        public static void ReloadEditingScenes() {
            int count = EditorPrefs.GetInt("OpenSceneCount", 0);

            for (int i = 0; i < count; i++) {
                string name = EditorPrefs.GetString($"OpenScenes[{i}].name", null);
                bool isLoaded = EditorPrefs.GetBool($"OpenScenes[{i}].loaded", true);
                if (string.IsNullOrEmpty(name)) continue;

                EditorSceneManager.OpenScene(name, i == 0 ? OpenSceneMode.Single : OpenSceneMode.Additive);
                if (!isLoaded) {
                    var loadedScene = SceneManager.GetSceneByName(name);
                    EditorSceneManager.CloseScene(loadedScene, false);
                }
            }

            SceneManager.SetActiveScene(SceneManager.GetSceneByPath(EditorPrefs.GetString("ActiveScene", null)));
        }
    }
}