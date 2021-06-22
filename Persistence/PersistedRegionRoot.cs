using System.Collections;
using System.Collections.Generic;

using SBR.Sequencing;

using UnityEngine;

namespace SBR.Persistence {
    public class PersistedRegionRoot : RegionRoot {
        public RegionSaveData SaveData { get; private set; }
        
        public override void Initialize() {
            base.Initialize();

            var level = PersistedLevelRoot.Current;
            if (!level) {
                Debug.LogError("Trying to initialize PersistedRegionRoot with no PersistedLevelRoot.");
                return;
            }

            SaveData = level.GetOrLoadRegionData(ManifestEntry);
        }
    }
}