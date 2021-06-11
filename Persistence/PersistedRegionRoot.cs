using System.Collections;
using System.Collections.Generic;

using SBR.Startup;

using UnityEngine;

namespace SBR.Persistence {
    public class PersistedRegionRoot : RegionRoot {
        public RegionSaveData SaveData { get; private set; }
        
        public override void Initialize() {
            base.Initialize();
            SaveData = PersistenceManager.Instance.GetRegionData(ManifestEntry.LevelID, ManifestEntry.RegionID);
        }
    }
}