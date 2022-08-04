using Infohazard.Core.Runtime;
using UnityEngine;

namespace SBR.Sequencing {
    public class ClearObjectPoolsStep : MonoBehaviour, IExecutionStep {
        public bool IsFinished => true;
        public void ExecuteForward(ExecutionStepArguments arguments) {
            PoolManager.Instance.ClearInactiveObjects();
        }
    }

}