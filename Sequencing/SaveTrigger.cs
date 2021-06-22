using SBR.Persistence;

using UnityEngine;

namespace SBR.Sequencing {
    public class SaveTrigger : MonoBehaviour {

        public void Activate() {
            var level = PersistedLevelRoot.Current;
            if (!level) {
                Debug.LogError("Cannot save with no active PersistedLevelRoot.");
                return;
            }
            
            level.Save();
        }
    }
}