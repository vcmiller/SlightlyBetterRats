using System.Collections.Generic;

using SBR.Sequencing;

using UnityEngine;

namespace SBR.Persistence {
    public class LoadStateSaveDataStep : MonoBehaviour, IExecutionStep {
        [SerializeField] private int _defaultStateIndex;

        public static readonly ExecutionStepParameter<int> ParamStateIndex = new ExecutionStepParameter<int>();
        
        public bool IsFinished => true;
        
        public void ExecuteForward(ExecutionStepArguments arguments) {
            int stateToLoad = ParamStateIndex.GetOrDefault(arguments, _defaultStateIndex);
            PersistenceManager.Instance.LoadStateData(stateToLoad);
        }
    }
}