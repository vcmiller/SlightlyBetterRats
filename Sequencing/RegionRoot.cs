using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SBR.Sequencing {
    public class RegionRoot : MonoBehaviour {
        [SerializeField] private LevelManifestRegionEntry _manifestEntry;

        public LevelManifestRegionEntry ManifestEntry => _manifestEntry;
        public int RegionIndex => _manifestEntry.RegionID;
        
        public virtual void Initialize() {
            
        }
    }
}