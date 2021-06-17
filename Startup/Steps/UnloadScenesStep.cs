using System.Collections;

using UnityEngine;

namespace SBR.Startup {
    public class UnloadScenesStep : MonoBehaviour, IExecutionStep {
        [SerializeField] private SceneGroup _groupToUnload;
        
        public bool IsFinished => true;
        
        public void ExecuteForward(ExecutionStepArguments arguments) {
            SceneLoadingManager.Instance.UnloadScenes(_groupToUnload);
        }
    }
}