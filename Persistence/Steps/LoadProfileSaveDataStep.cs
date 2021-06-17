using System.Collections.Generic;

using SBR.Startup;

using UnityEngine;

namespace SBR.Persistence {
    public class LoadProfileSaveDataStep : MonoBehaviour, IExecutionStep {
        [SerializeField] private string _defaultProfileName = "Default";

        public static readonly ExecutionStepParameter<string> ParamProfileName = new ExecutionStepParameter<string>();
        
        public bool IsFinished => true;
        
        public void ExecuteForward(ExecutionStepArguments arguments) {
            string profileToLoad = ParamProfileName.GetOrDefault(arguments, _defaultProfileName);
            PersistenceManager.Instance.LoadProfileData(profileToLoad);
        }
    }
}