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
        [SerializeField] private SceneGroup _sceneGroup;

        public static readonly ExecutionStepParameter<string> ParamSceneToLoad = new ExecutionStepParameter<string>();
        
        public bool IsFinished { get; private set; }
        
        public void ExecuteForward(ExecutionStepArguments arguments) {
            StartCoroutine(CRT_Execution(arguments));
        }

        private IEnumerator CRT_Execution(ExecutionStepArguments arguments) {
            IsFinished = false;

            string sceneToLoad = ParamSceneToLoad.GetOrDefault(arguments, _defaultSceneToLoad.Name);
            AsyncOperation operation = SceneLoadingManager.Instance.LoadScene(sceneToLoad, _enableImmediately,
                                                                         _makeActiveScene, _sceneGroup);
            if (operation == null) yield break;
            
            var level = LevelManifest.Instance.GetLevelWithSceneName(sceneToLoad);
            LoadInitialRegionsStep.ParamLoadingLevel.Set(arguments, level);

            if (_enableImmediately) {
                while (operation.progress < 0.9f) {
                    yield return null;
                }
            } else {
                yield return operation;
            }

            IsFinished = true;
        }
    }
}
