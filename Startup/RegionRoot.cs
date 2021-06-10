using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SBR.Startup {
    public class RegionRoot : MonoBehaviour {
        [SerializeField] private LevelManifestRegionEntry _manifestEntry;

        public LevelManifestRegionEntry ManifestEntry => _manifestEntry;
        public int RegionIndex => _manifestEntry.RegionID;
    }
}