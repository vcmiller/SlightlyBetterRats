using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SBR.Startup {
    public class InstantEventStep : MonoBehaviour, IExecutionStep {
        [SerializeField] private UnityEvent _onExecute;
        
        public bool IsFinished => true;
        
        public void ExecuteForward(ExecutionStepArguments arguments) {
            _onExecute?.Invoke();
        }
    }
}