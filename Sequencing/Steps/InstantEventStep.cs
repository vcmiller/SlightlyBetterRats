using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SBR.Sequencing {
    public class InstantEventStep : MonoBehaviour, IExecutionStep {
        [SerializeField] private UnityEvent _onExecute;
        
        public bool IsFinished => true;
        
        public void ExecuteForward(ExecutionStepArguments arguments) {
            Debug.Log($"InstantEvent execute: {name}", this);
            _onExecute?.Invoke();
        }
    }
}