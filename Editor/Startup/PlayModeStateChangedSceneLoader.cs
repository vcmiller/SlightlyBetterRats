using System.Collections.Generic;

using SBR.Startup;

using UnityEditor;
using UnityEditor.SceneManagement;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace SBR.Editor.Startup {
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
                EditorPrefs.SetString("ActiveScene", SceneManager.GetActiveScene().path);
                EditorPrefs.SetInt("OpenSceneCount", SceneManager.sceneCount);
                for (int i = 0; i < SceneManager.sceneCount; i++) {
                    var scene = SceneManager.GetSceneAt(i);
                    EditorPrefs.SetString($"OpenScenes[{i}].name", scene.path);
                    EditorPrefs.SetBool($"OpenScenes[{i}].loaded", scene.isLoaded);
                }

                var levelRoot = Object.FindObjectOfType<LevelRoot>();
                if (levelRoot) {
                    EditorPrefs.SetInt("OpenLevel", levelRoot.LevelIndex);
                    var regions = levelRoot.RegionScenes;
                    for (int i = 0; i < regions.Count; i++) {
                        EditorPrefs.SetBool($"OpenLevelRegions[{i}].loaded",
                                            SceneManager.GetSceneByPath(regions[i].ScenePath).isLoaded);
                    }
                } else {
                    int openLevel = -1;
                    foreach (RegionRoot regionRoot in Object.FindObjectsOfType<RegionRoot>()) {
                        if (openLevel == -1 && regionRoot.LevelIndex >= 0) {
                            openLevel = regionRoot.LevelIndex;
                        }

                        if (openLevel >= 0 && regionRoot.LevelIndex == openLevel) {
                            EditorPrefs.SetBool($"OpenLevelRegions[{regionRoot.RegionIndex}].loaded", true);
                        }
                    }
                    EditorPrefs.SetInt("OpenLevel", openLevel);
                }

                EditorSceneManager.OpenScene(EditorBuildSettings.scenes[0].path, OpenSceneMode.Single);
            } else if (mode == PlayModeStateChange.EnteredEditMode) {
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
}