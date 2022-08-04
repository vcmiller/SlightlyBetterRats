using UnityEngine;

namespace SBR.Sequencing {
    public class ExternalSequencerStep : MonoBehaviour, IExecutionStep {
        [SerializeField] private ExecutionStepSequencer _sequencer;
        
        public bool IsFinished => _sequencer.IsFinished;
        public void ExecuteForward(ExecutionStepArguments arguments) => _sequencer.ExecuteForward(arguments);
    }
}
