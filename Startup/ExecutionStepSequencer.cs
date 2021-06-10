using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SBR.Startup {
    public class ExecutionStepSequencer : MonoBehaviour {
        [SerializeField] private bool _playOnAwake = true;
        
        private List<IExecutionStep> _steps;

        public static bool InitialSceneIsLoaded {
            get {
                for (int i = 0; i < SceneManager.sceneCount; i++) {
                    var scene = SceneManager.GetSceneAt(i);
                    if (scene.buildIndex == 0) return true;
                }

                return false;
            }
        }
        
        private void Start() {
            if (!InitialSceneIsLoaded) {
                Debug.LogError("Initial scene is not loaded!");
                SceneControl.Quit();
            }

            _steps = new List<IExecutionStep>();
            for (int i = 0; i < transform.childCount; i++) {
                if (transform.GetChild(i).TryGetComponent(out IExecutionStep step)) {
                    _steps.Add(step);
                }
            }

            if (_playOnAwake) {
                Play();
            }
        }

        public void Play() {
            StartCoroutine(PlayCoroutine());
        }

        public IEnumerator PlayCoroutine() {
            for (int index = 0; index < _steps.Count; index++) {
                IExecutionStep step = _steps[index];
                step.ExecuteForward();
                while (!step.IsFinished) {
                    yield return null;
                }
            }
        }
    }

    public interface IExecutionStep {
        bool IsFinished { get; }
        
        void ExecuteForward();
    }
}