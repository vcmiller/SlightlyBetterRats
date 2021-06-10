using System.Collections;
using System.Collections.Generic;

using SBR.Startup;

using UnityEngine;

namespace SBR.Persistence {
    public class PersistedLevelRoot : LevelRoot {
        public LevelSaveData SaveData { get; private set; }

        public override void Initialize() {
            base.Initialize();
            SaveData = PersistenceManager.Instance.GetLevelData(ManifestEntry.LevelID);
        }
    }
}
