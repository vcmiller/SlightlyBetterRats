using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace SBR.Sequencing {
    public class SceneLoadingManager : Singleton<SceneLoadingManager> {
        private Dictionary<SceneGroup, SceneGroupInfo> _sceneGroups = new Dictionary<SceneGroup, SceneGroupInfo>();
        private Dictionary<string, SceneLoadingState> _sceneLoadingStates = new Dictionary<string, SceneLoadingState>();

        [SerializeField] private SceneGroup _defaultGroup;

        public bool IsUnloadingAnyScenes => _sceneGroups.Values.Any(g => g.UnloadingScenes.Count > 0);

        public bool IsLoadingAnyScenes(SceneLoadingType type = SceneLoadingType.All) =>
            _sceneGroups.Values.Any(g => g.LoadingScenes.Any(op => (op.SceneType & type) != 0));
        
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

            if (_sceneLoadingStates.TryGetValue(sceneName, out SceneLoadingState state) &&
                (state == SceneLoadingState.Loaded || state == SceneLoadingState.Loading)) {
                return null;
            }

            if (state == SceneLoadingState.Cancelled) {
                foreach (SceneLoadingOperation loadingScene in groupInfo.LoadingScenes) {
                    if (loadingScene.SceneName != sceneName) continue;
                    loadingScene.IsCancelled = false;
                    loadingScene.SetActiveOnComplete = setActiveScene;
                    loadingScene.Operation.allowSceneActivation = autoActivate;
                    _sceneLoadingStates[sceneName] = SceneLoadingState.Loading;
                    return loadingScene.Operation;
                }
            }
            
            AsyncOperation op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            if (op == null) return null;
            _sceneLoadingStates[sceneName] = SceneLoadingState.Loading;
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
                _sceneLoadingStates[loadingScene.SceneName] = SceneLoadingState.Cancelled;
            }

            foreach (Scene scene in groupInfo.LoadedScenes) {
                AsyncOperation op = SceneManager.UnloadSceneAsync(scene);
                groupInfo.UnloadingScenes.Add(new SceneUnloadingOperation {
                    Operation = op,
                    SceneName = scene.name,
                });
                _sceneLoadingStates[scene.name] = SceneLoadingState.Unloading;
            }
            groupInfo.LoadedScenes.Clear();
        }

        public bool UnloadScene(string sceneName) {
            if (_sceneLoadingStates.TryGetValue(sceneName, out SceneLoadingState state) &&
                (state == SceneLoadingState.Unloaded ||
                 state == SceneLoadingState.Cancelled ||
                 state == SceneLoadingState.Unloading)) {
                return false;
            }

            if (state == SceneLoadingState.Loaded) {
                foreach (SceneGroupInfo groupInfo in _sceneGroups.Values) {
                    for (int index = 0; index < groupInfo.LoadedScenes.Count; index++) {
                        Scene scene = groupInfo.LoadedScenes[index];
                        if (scene.name != sceneName) continue;

                        groupInfo.LoadedScenes.Remove(scene);
                        AsyncOperation op = SceneManager.UnloadSceneAsync(scene);
                        groupInfo.UnloadingScenes.Add(new SceneUnloadingOperation {
                            Operation = op,
                            SceneName = scene.name,
                        });
                        return true;
                    }
                }
            } else if (state == SceneLoadingState.Loading) {
                foreach (SceneGroupInfo groupInfo in _sceneGroups.Values) {
                    foreach (SceneLoadingOperation loadingScene in groupInfo.LoadingScenes) {
                        if (loadingScene.SceneName != sceneName) continue;
                        loadingScene.IsCancelled = true;
                        return true;
                    }
                }
            }

            Debug.LogError($"Invalid state: scene {sceneName} state is set to {state}, but could not find scene.");
            return false;
        }

        public void ActivateLoadingScenes(SceneGroup group = null, SceneLoadingType typesToActivate = SceneLoadingType.All) {
            if (!group) group = _defaultGroup;
            if (!_sceneGroups.TryGetValue(group, out SceneGroupInfo groupInfo)) return;

            foreach (SceneLoadingOperation op in groupInfo.LoadingScenes) {
                if ((op.SceneType & typesToActivate) != 0) {
                    op.Operation.allowSceneActivation = true;
                }
            }
        }

        private void Update() {
            foreach (SceneGroupInfo groupInfo in _sceneGroups.Values) {
                foreach (SceneLoadingOperation operation in groupInfo.LoadingScenes) {
                    if (!operation.Operation.isDone) continue;
                    
                    Scene scene = SceneManager.GetSceneByName(operation.SceneName);
                    if (!scene.isLoaded) {
                        Debug.LogError($"Unexpected: scene {operation.SceneName} not loaded after operation complete.");
                        continue;
                    }

                    if (operation.IsCancelled) {
                        AsyncOperation op = SceneManager.UnloadSceneAsync(scene);
                        groupInfo.UnloadingScenes.Add(new SceneUnloadingOperation {
                            Operation = op,
                            SceneName = scene.name,
                        });
                        _sceneLoadingStates[scene.name] = SceneLoadingState.Unloading;
                        continue;
                    }
                        
                    groupInfo.LoadedScenes.Add(scene);
                    _sceneLoadingStates[scene.name] = SceneLoadingState.Loaded;
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

                foreach (SceneUnloadingOperation operation in groupInfo.UnloadingScenes) {
                    if (!operation.Operation.isDone) continue;
                    _sceneLoadingStates[operation.SceneName] = SceneLoadingState.Unloaded;
                }

                groupInfo.LoadingScenes.RemoveAll(op => op.Operation.isDone);
                groupInfo.UnloadingScenes.RemoveAll(op => op.Operation.isDone);
            }
        }

        private class SceneGroupInfo {
            public List<SceneLoadingOperation> LoadingScenes { get; } = new List<SceneLoadingOperation>();
            public List<Scene> LoadedScenes { get; } = new List<Scene>();
            public List<SceneUnloadingOperation> UnloadingScenes { get; } = new List<SceneUnloadingOperation>();
        }
        
        private class SceneLoadingOperation {
            public AsyncOperation Operation { get; set; }
            public string SceneName { get; set; }
            public bool SetActiveOnComplete { get; set; }
            public SceneLoadingType SceneType { get; set; }
            public bool IsCancelled { get; set; }
        }
        
        private class SceneUnloadingOperation {
            public AsyncOperation Operation { get; set; }
            public string SceneName { get; set; }
        }
    }

    public enum SceneLoadingState {
        Unloaded, Loading, Cancelled, Loaded, Unloading
    }

    [Flags]
    public enum SceneLoadingType {
        Scene = 1 << 0,
        Level = 1 << 1,
        Region = 1 << 2,
        
        All = Scene | Level | Region,
    }
}