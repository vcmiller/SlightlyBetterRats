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

        internal virtual void RegisterRegion(RegionRoot region) {
            _loadedRegions.Add(region);
        }

        internal virtual void DeregisterRegion(RegionRoot region) {
            _loadedRegions.Remove(region);
        }

        public virtual void Initialize() {
            if (Current) {
                Debug.LogError("Trying to initialize a LevelRoot when one is already active!");
                return;
            }

            Current = this;
        }

        public virtual void Cleanup() {
            if (Current == this) {
                Current = null;
            }
        }

        public virtual bool LoadRegion(LevelManifestRegionEntry regionEntry, SceneGroup group = null) {
            if (regionEntry.AlwaysLoaded) {
                Debug.LogError($"Region {regionEntry.name} is always loaded, and cannot be passed as an argument to LoadRegion.");
                return false;
            }
            AsyncOperation op = SceneLoadingManager.Instance.LoadScene(regionEntry.Scene.Name, true, false, group);
            return op != null;
        }

        public virtual bool UnloadRegion(LevelManifestRegionEntry regionEntry) {
            if (regionEntry.AlwaysLoaded) {
                Debug.LogError($"Region {regionEntry.name} is always loaded, and cannot be passed as an argument to UnloadRegion.");
                return false;
            }
            return SceneLoadingManager.Instance.UnloadScene(regionEntry.Scene.Name);
        }
    }
}