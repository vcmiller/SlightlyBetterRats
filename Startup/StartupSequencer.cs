using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SBR.Startup {
    public class StartupSequencer : MonoBehaviour
    {
        private void Awake() {
            if (!InitialSceneIsLoaded) {
                Debug.LogError("Initial scene is not loaded!");
                SceneControl.Quit();
            }
        }

        public static bool InitialSceneIsLoaded {
            get {
                for (int i = 0; i < SceneManager.sceneCount; i++) {
                    var scene = SceneManager.GetSceneAt(i);
                    if (scene.buildIndex == 0) return true;
                }

                return false;
            }
        }
    }

}