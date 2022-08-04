using Infohazard.Core.Runtime;
using UnityEngine;

namespace SBR.Sequencing {
    public class PassSceneStep : MonoBehaviour, IExecutionStep {
        [SerializeField] private SceneRef _sceneToLoad;
        public bool IsFinished => true;
        
        public void ExecuteForward(ExecutionStepArguments arguments) {
            LoadSceneOrLevelStep.ParamSceneToLoad.Set(arguments, _sceneToLoad.Name);
        }
    }
}