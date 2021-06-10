using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SBR.Startup {
    public class OpenSceneBranchStep : MonoBehaviour, IExecutionStep {
        [SerializeField] private ExecutionStepSequencer _mainPath;
        [SerializeField] private ExecutionStepSequencer _openScenePath;
        [SerializeField] private ExecutionStepSequencer _openLevelPath;
        
        public bool IsFinished { get; }

        private ExecutionStepSequencer _executingSequencer;
        
        public void ExecuteForward(ExecutionStepArguments arguments) {
            throw new System.NotImplementedException();
        }
    }
}