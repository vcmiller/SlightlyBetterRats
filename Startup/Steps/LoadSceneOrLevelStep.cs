using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace SBR.Startup {
    public class LoadSceneOrLevelStep : MonoBehaviour, IExecutionStep {
        [SerializeField] private SceneRef _defaultSceneToLoad;
        [SerializeField] private bool _makeActiveScene;
        [SerializeField] private bool _enableImmediately;

        public static readonly ExecutionStepParameter<string> ParamSceneToLoad = new ExecutionStepParameter<string>();

        public static readonly ExecutionStepParameter<IEnumerable<int>> ParamRegionsToLoad =
            new ExecutionStepParameter<IEnumerable<int>>();
        
        public bool IsFinished { get; private set; }

        private AsyncOperation _operation;
        private List<AsyncOperation> _regionOperations;

        private string _sceneLoading;
        
        public void ExecuteForward(ExecutionStepArguments arguments) {
            StartCoroutine(CRT_Execution(arguments));
        }

        private IEnumerator CRT_Execution(ExecutionStepArguments arguments) {
            IsFinished = false;

            _sceneLoading = ParamSceneToLoad.GetOrDefault(arguments, _defaultSceneToLoad.Name);
            _operation = SceneManager.LoadSceneAsync(_sceneLoading, LoadSceneMode.Additive);
            _operation.allowSceneActivation = false;
            
            var level = LevelManifest.Instance.GetLevelWithSceneName(_sceneLoading);
            if (level) {
                _regionOperations = new List<AsyncOperation>();

                foreach (int regionID in ParamRegionsToLoad.GetOrDefault(arguments, DefaultRegionsToLoad(level))) {
                    var region = level.GetRegionWithID(regionID);
                    if (!region) continue;

                    AsyncOperation regionOperation =
                        SceneManager.LoadSceneAsync(region.Scene.Name, LoadSceneMode.Additive);
                    regionOperation.allowSceneActivation = false;
                    _regionOperations.Add(regionOperation);
                }
            } else {
                _regionOperations = null;
            }
            
            while (_operation.progress < 0.9f || (_regionOperations != null && _regionOperations.Any(op => op.progress < 0.9f))) {
                yield return null;
            }

            if (_enableImmediately) {
                EnableScene();
            }

            IsFinished = true;
        }

        private IEnumerable<int> DefaultRegionsToLoad(LevelManifestLevelEntry level) {
            foreach (LevelManifestRegionEntry region in level.Regions) {
                if (region.LoadedByDefault) yield return region.RegionID;
            }
        }

        public void EnableScene() {
            if (_operation == null || _operation.allowSceneActivation) return;
            _operation.allowSceneActivation = true;

            if (_regionOperations != null) {
                foreach (AsyncOperation operation in _regionOperations) {
                    operation.allowSceneActivation = true;
                }
            }
            
            if (_makeActiveScene) {
                StartCoroutine(CRT_SetActive());
            }
        }

        private IEnumerator CRT_SetActive() {
            yield return _operation;
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(_sceneLoading));
        }
    }
}
