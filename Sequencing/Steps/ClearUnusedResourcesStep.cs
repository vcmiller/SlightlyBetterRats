using System.Collections;
using System.Collections.Generic;

using SBR.Sequencing;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace SBR.Sequencing {
    public class ClearUnusedResourcesStep : MonoBehaviour, IExecutionStep {
        public bool IsFinished => true;
        public void ExecuteForward(ExecutionStepArguments arguments) {
            Resources.UnloadUnusedAssets();
        }
    }

}