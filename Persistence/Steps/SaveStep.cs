using System.Collections;
using System.Collections.Generic;

using SBR.Sequencing;

using UnityEngine;

namespace SBR.Persistence {
    public class SaveStep : MonoBehaviour, IExecutionStep {
        [SerializeField] private bool _saveGlobal = true;
        [SerializeField] private bool _saveProfile = true;
        [SerializeField] private bool _saveState = true;
        
        public bool IsFinished => true;
        public void ExecuteForward(ExecutionStepArguments arguments) {
            if (_saveState) PersistenceManager.Instance.SaveStateData();
            if (_saveProfile) PersistenceManager.Instance.SaveProfileData();
            if (_saveGlobal) PersistenceManager.Instance.SaveGlobalData();
        }
    }
}
