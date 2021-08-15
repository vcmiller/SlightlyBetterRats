using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SBR.Sequencing {
    public class RegionRoot : MonoBehaviour {
        [SerializeField] private LevelManifestRegionEntry _manifestEntry;

        public LevelManifestRegionEntry ManifestEntry => _manifestEntry;

        public int RegionIndex => _manifestEntry.RegionID;
        
        public bool Initialized { get; private set; }
        
        public virtual void Initialize() {
            LevelRoot.Current.RegisterRegion(this);
            Initialized = true;
        }

        public virtual void Cleanup() {
            Initialized = false;
            // It's ok if the level is unloaded before its regions (as long as it's in the same frame)
            if (LevelRoot.Current) {
                LevelRoot.Current.DeregisterRegion(this);
            }
        }
    }
}