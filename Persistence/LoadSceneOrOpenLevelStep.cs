using System.Collections;
using System.Collections.Generic;
using System.IO;

using SBR.Startup;

using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace SBR.Persistence {
    public class LoadSceneOrOpenLevelStep : SimpleLoadSceneStep {
        protected override string SceneNameToLoad {
            get {
#if UNITY_EDITOR
                string activeScene = EditorPrefs.GetString("ActiveScene", null);
                if (!string.IsNullOrEmpty(activeScene)) {
                    for (int index = 1; index < EditorBuildSettings.scenes.Length; index++) {
                        EditorBuildSettingsScene scene = EditorBuildSettings.scenes[index];
                        if (scene.path == activeScene) {
                            return Path.GetFileNameWithoutExtension(activeScene);
                        }
                    }
                }
#endif
                
                return base.SceneNameToLoad;
            }
        }
    }
}