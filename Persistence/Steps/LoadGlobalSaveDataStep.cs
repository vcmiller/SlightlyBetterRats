using System.Collections;
using System.Collections.Generic;

using SBR.Sequencing;

using UnityEngine;

namespace SBR.Persistence {
    public class LoadGlobalSaveDataStep : MonoBehaviour, IExecutionStep {
        public bool IsFinished => true;
        public void ExecuteForward(ExecutionStepArguments arguments) {
            PersistenceManager.Instance.LoadGlobalData();
        }
    }
}
