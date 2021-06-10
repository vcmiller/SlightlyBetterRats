using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace SBR.Startup {
    public class SimpleLoadSceneStep : MonoBehaviour, IExecutionStep {
        [SerializeField] private SceneRef _sceneToLoad;
        [SerializeField] private bool _makeActiveScene;
        [SerializeField] private bool _enableImmediately;

        private AsyncOperation _operation;

        public bool IsFinished { get; private set; }
        protected virtual string SceneNameToLoad => _sceneToLoad.Name;
        private string _sceneLoading;
        
        public void ExecuteForward() {
            StartCoroutine(CRT_Execution());
        }

        private IEnumerator CRT_Execution() {
            IsFinished = false;

            _sceneLoading = SceneNameToLoad;
            _operation = SceneManager.LoadSceneAsync(_sceneLoading, LoadSceneMode.Additive);
            _operation.allowSceneActivation = false;
            while (_operation.progress < 0.9f) {
                yield return null;
            }

            if (_enableImmediately) {
                EnableScene();
            }

            IsFinished = true;
        }

        public void EnableScene() {
            if (_operation == null || _operation.allowSceneActivation) return;
            _operation.allowSceneActivation = true;
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
