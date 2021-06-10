using System.Collections;
using System.Collections.Generic;

using SBR.Startup;

using UnityEngine;

namespace SBR.Persistence {
    public class PersistedLevelRoot : LevelRoot {
        public LevelSaveData SaveData { get; private set; }

        protected override IEnumerable<AsyncOperation> LoadInitialRegions(bool autoActivate) {
            if (SaveData == null) {
                Debug.LogError("Trying to load active regions before level save data is loaded.");
                yield break;
            }
            
            for (int i = 0; i < RegionScenes.Count; i++) {
                AsyncOperation op = SetRegionLoaded(i, SaveData.IsRegionLoaded(i), autoActivate);
                if (op != null) yield return op;
            }
        }

        public override AsyncOperation SetRegionLoaded(int regionIndex, bool loaded, bool autoActivate) {
            AsyncOperation op = base.SetRegionLoaded(regionIndex, loaded, autoActivate);
            if (op != null) {
                SaveData.SetRegionLoaded(regionIndex, loaded);
            }

            return op;
        }
    }
}
