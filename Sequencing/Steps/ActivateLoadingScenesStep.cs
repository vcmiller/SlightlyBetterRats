using UnityEngine;
using UnityEngine.Serialization;

namespace SBR.Sequencing {
    public class ActivateLoadingScenesStep : MonoBehaviour, IExecutionStep {
        [SerializeField] private SceneGroup _groupToActivate;
        
        public bool IsFinished => true;
        
        public void ExecuteForward(ExecutionStepArguments arguments) {
            SceneLoadingManager.Instance.ActivateLoadingScenes(_groupToActivate);
        }
    }
}