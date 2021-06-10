using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SBR.Startup {
    public class ExternalSequencerStep : MonoBehaviour, IExecutionStep {
        [SerializeField] private ExecutionStepSequencer _sequencer;
        
        public bool IsFinished => _sequencer.IsFinished;
        public void ExecuteForward(ExecutionStepArguments arguments) => _sequencer.ExecuteForward(arguments);
    }
}
