using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace SBR.Sequencing {
    public class LevelRoot : MonoBehaviour {
        public static LevelRoot Current { get; private set; }
        
        [SerializeField] private LevelManifestLevelEntry _manifestEntry;
        
        private List<RegionRoot> _loadedRegions = new List<RegionRoot>();
        
        public int LevelIndex => _manifestEntry.LevelID;
        public LevelManifestLevelEntry ManifestEntry => _manifestEntry;
        public IReadOnlyList<RegionRoot> LoadedRegions => _loadedRegions;

        internal void RegisterRegion(RegionRoot region) {
            _loadedRegions.Add(region);
        }

        internal void DeregisterRegion(RegionRoot region) {
            _loadedRegions.Remove(region);
        }

        public virtual void Initialize() {
            if (Current) {
                Debug.LogError("Trying to initialize a LevelRoot when one is already active!");
                return;
            }

            Current = this;
        }

        public virtual void OnDestroy() {
            if (Current == this) {
                Current = null;
            }
        }

        public bool LoadRegion(LevelManifestRegionEntry regionEntry, SceneGroup group = null) {
            AsyncOperation op = SceneLoadingManager.Instance.LoadScene(regionEntry.Scene.Name, true, false, group);
            return op != null;
        }

        public bool UnloadRegion(LevelManifestRegionEntry regionEntry) {
            return SceneLoadingManager.Instance.UnloadScene(regionEntry.Scene.Name);
        }
    }
}