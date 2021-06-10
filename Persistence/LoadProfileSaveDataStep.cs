using SBR.Startup;

using UnityEngine;

namespace SBR.Persistence {
    public class LoadProfileSaveDataStep : MonoBehaviour, IExecutionStep {
        [SerializeField] private string _profileName;
        
        public bool IsFinished => true;
        
        public void ExecuteForward() {
            PersistenceManager.Instance.LoadProfileData(_profileName);
        }
    }
}