using System.Collections;

using UnityEngine;

namespace SBR.Startup {
    public class WaitForScenesToUnloadStep : MonoBehaviour, IExecutionStep {
        public bool IsFinished => !SceneLoadingManager.Instance.IsUnloadingAnyScenes;
        public void ExecuteForward(ExecutionStepArguments arguments) { }
    }
}