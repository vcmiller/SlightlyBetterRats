using System.Collections;
using System.Collections.Generic;

using SBR.Sequencing;

using UnityEngine;

namespace SBR.Persistence {
    public class InitializeObjectsStep : MonoBehaviour, IExecutionStep {
        public bool IsFinished { get; private set; }
        
        public void ExecuteForward(ExecutionStepArguments arguments) {
            var level = PersistedLevelRoot.Current;
            if (!level) return;

            IsFinished = false;
            StartCoroutine(CRT_Execution(level));
        }

        private IEnumerator CRT_Execution(PersistedLevelRoot level) {
            yield return null;
            level.LoadObjects();
            IsFinished = true;
        }
    }
}