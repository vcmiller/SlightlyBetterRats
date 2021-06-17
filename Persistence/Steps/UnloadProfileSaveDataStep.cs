using SBR.Sequencing;

using UnityEngine;

namespace SBR.Persistence {
    public class 
        UnloadProfileSaveDataStep : MonoBehaviour, IExecutionStep {
        public bool IsFinished => true;
        
        public void ExecuteForward(ExecutionStepArguments arguments) {
            PersistenceManager.Instance.UnloadProfileData();
        }
    }
}