using UnityEngine;

namespace SBR.Sequencing {
    public class WaitForScenesToUnloadStep : MonoBehaviour, IExecutionStep {
        public bool IsFinished => !SceneLoadingManager.Instance.IsUnloadingAnyScenes;
        public void ExecuteForward(ExecutionStepArguments arguments) { }
    }
}