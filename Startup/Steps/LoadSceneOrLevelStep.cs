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
        
        public bool IsFinished { get; private set; }

        private AsyncOperation _operation;
        private bool _loadingLevel;
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
            _loadingLevel = level != null;
            LoadInitialRegionsStep.ParamLoadingLevel.Set(arguments, level);
            
            while (_operation.progress < 0.9f) {
                yield return null;
            }

            if (_enableImmediately) {
                yield return EnableScene();
            }

            IsFinished = true;
        }

        public Coroutine EnableScene() {
            if (_operation == null || _operation.allowSceneActivation) return null;
            _operation.allowSceneActivation = true;
            
            return StartCoroutine(CRT_ActivateScene(_makeActiveScene));
        }

        private IEnumerator CRT_ActivateScene(bool makeActive) {
            yield return _operation;

            var scene = SceneManager.GetSceneByName(_sceneLoading);
            if (_makeActiveScene) {
                SceneManager.SetActiveScene(scene);
            }
            if (_loadingLevel) {
                foreach (GameObject obj in scene.GetRootGameObjects()) {
                    if (!obj.TryGetComponentInChildren(out LevelRoot level)) continue;
                    level.Initialize();
                    break;
                }
            }
        }
    }
}
