using System.Collections;
using System.Collections.Generic;

using SBR.Startup;

using UnityEngine;

namespace SBR.Persistence {
    public class LoadGlobalSaveDataStep : MonoBehaviour, IExecutionStep {
        public bool IsFinished => true;
        public void ExecuteForward() {
            PersistenceManager.Instance.LoadGlobalData();
        }
    }
}
