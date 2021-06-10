using SBR.Startup;

using UnityEngine;

namespace SBR.Persistence {
    public class LoadStateSaveDataStep : MonoBehaviour, IExecutionStep {
        [SerializeField] private int _stateIndex;
        
        public bool IsFinished => true;
        
        public void ExecuteForward() {
            PersistenceManager.Instance.LoadStateData(_stateIndex);
        }
    }
}