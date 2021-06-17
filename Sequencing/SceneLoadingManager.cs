using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace SBR.Sequencing {
    public class SceneLoadingManager : Singleton<SceneLoadingManager> {
        private Dictionary<SceneGroup, SceneGroupInfo> _sceneGroups = new Dictionary<SceneGroup, SceneGroupInfo>();

        [SerializeField] private SceneGroup _defaultGroup;

        public bool IsUnloadingAnyScenes => _sceneGroups.Values.Any(g => g.UnloadingScenes.Count > 0);
        
        public List<AsyncOperation> LoadScenes(IEnumerable<string> scenes, bool autoActivate, SceneGroup group = null) {
            List<AsyncOperation> ops = new List<AsyncOperation>();
            foreach (string scene in scenes) {
                AsyncOperation op = LoadScene(scene, autoActivate, false, group);
                if (op != null) ops.Add(op);
            }

            return ops;
        }

        public AsyncOperation LoadScene(string sceneName, bool autoActivate, bool setActiveScene, SceneGroup group = null) {
            if (!group) group = _defaultGroup;
            if (!_sceneGroups.TryGetValue(group, out SceneGroupInfo groupInfo)) {
                groupInfo = new SceneGroupInfo();
                _sceneGroups.Add(group, groupInfo);
            }

            AsyncOperation op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            if (op == null) return null;
            op.allowSceneActivation = autoActivate;

            SceneLoadingType type = LevelManifest.Instance.GetLevelWithSceneName(sceneName)
                ? SceneLoadingType.Level
                : LevelManifest.Instance.Levels.Any(l => l.GetRegionWithSceneName(sceneName))
                    ? SceneLoadingType.Region
                    : SceneLoadingType.Scene;
            
            groupInfo.LoadingScenes.Add(new SceneLoadingOperation {
                Operation = op,
                SceneName = sceneName,
                SetActiveOnComplete = setActiveScene,
                SceneType = type,
            });

            return op;
        }

        public void UnloadScenes(SceneGroup group = null) {
            if (!group) group = _defaultGroup;
            if (!_sceneGroups.TryGetValue(group, out SceneGroupInfo groupInfo)) return;

            foreach (SceneLoadingOperation loadingScene in groupInfo.LoadingScenes) {
                loadingScene.IsCancelled = true;
            }

            foreach (Scene scene in groupInfo.LoadedScenes) {
                AsyncOperation op = SceneManager.UnloadSceneAsync(scene);
                groupInfo.UnloadingScenes.Add(op);
            }
            groupInfo.LoadedScenes.Clear();
        }

        public bool UnloadScene(string sceneName) {
            foreach (SceneGroupInfo groupInfo in _sceneGroups.Values) {
                for (int index = 0; index < groupInfo.LoadedScenes.Count; index++) {
                    Scene scene = groupInfo.LoadedScenes[index];
                    if (scene.name != sceneName) continue;

                    groupInfo.LoadedScenes.Remove(scene);
                    AsyncOperation op = SceneManager.UnloadSceneAsync(scene);
                    groupInfo.UnloadingScenes.Add(op);
                    return true;
                }

                foreach (SceneLoadingOperation loadingScene in groupInfo.LoadingScenes) {
                    if (loadingScene.SceneName != sceneName) continue;
                    loadingScene.IsCancelled = true;
                    return true;
                }
            }

            return false;
        }

        public void ActivateLoadingScenes(SceneGroup group = null) {
            if (!group) group = _defaultGroup;
            if (!_sceneGroups.TryGetValue(group, out SceneGroupInfo groupInfo)) return;

            foreach (SceneLoadingOperation op in groupInfo.LoadingScenes) {
                op.Operation.allowSceneActivation = true;
            }
        }

        private void Update() {
            foreach (SceneGroupInfo groupInfo in _sceneGroups.Values) {
                foreach (SceneLoadingOperation operation in groupInfo.LoadingScenes) {
                    if (operation.Operation.isDone) {
                        Scene scene = SceneManager.GetSceneByName(operation.SceneName);
                        if (!scene.isLoaded) {
                            Debug.LogError($"Unexpected: scene {operation.SceneName} not loaded after operation complete.");
                            continue;
                        }

                        if (operation.IsCancelled) {
                            groupInfo.UnloadingScenes.Add(SceneManager.UnloadSceneAsync(scene));
                            continue;
                        }
                        
                        groupInfo.LoadedScenes.Add(scene);
                        if (operation.SetActiveOnComplete) {
                            SceneManager.SetActiveScene(scene);
                        }

                        if (operation.SceneType == SceneLoadingType.Level) {
                            foreach (GameObject obj in scene.GetRootGameObjects()) {
                                if (!obj.TryGetComponent(out LevelRoot level)) continue;
                                level.Initialize();
                                break;
                            }
                        } else if (operation.SceneType == SceneLoadingType.Region) {
                            foreach (GameObject obj in scene.GetRootGameObjects()) {
                                if (!obj.TryGetComponent(out RegionRoot region)) continue;
                                region.Initialize();
                                break;
                            }
                        }
                    }
                }

                groupInfo.LoadingScenes.RemoveAll(op => op.Operation.isDone);
                groupInfo.UnloadingScenes.RemoveAll(op => op.isDone);
            }
        }

        private class SceneGroupInfo {
            public List<SceneLoadingOperation> LoadingScenes { get; } = new List<SceneLoadingOperation>();
            public List<Scene> LoadedScenes { get; } = new List<Scene>();
            public List<AsyncOperation> UnloadingScenes { get; } = new List<AsyncOperation>();
        }
        
        private class SceneLoadingOperation {
            public AsyncOperation Operation { get; set; }
            public string SceneName { get; set; }
            public bool SetActiveOnComplete { get; set; }
            public SceneLoadingType SceneType { get; set; }
            public bool IsCancelled { get; set; }
        }

        private enum SceneLoadingType {
            Scene, Level, Region,
        }
    }
}