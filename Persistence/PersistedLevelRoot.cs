using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using SBR.Sequencing;

using UnityEngine;

namespace SBR.Persistence {
    public class PersistedLevelRoot : LevelRoot {
        public LevelSaveData SaveData { get; private set; }
        public new static PersistedLevelRoot Current => LevelRoot.Current as PersistedLevelRoot;
        
        private bool _dirty;

        private Dictionary<int, LoadedRegionInfo> _loadedRegionData = new Dictionary<int, LoadedRegionInfo>();

        public override void Initialize() {
            base.Initialize();
            SaveData = PersistenceManager.Instance.GetLevelData(ManifestEntry.LevelID);
            SaveData.StateChanged += SaveData_StateChanged;
        }

        public override void Cleanup() {
            SaveData.StateChanged -= SaveData_StateChanged;
            base.Cleanup();
        }

        internal override void RegisterRegion(RegionRoot region) {
            base.RegisterRegion(region);
            GetOrLoadRegionData(region.ManifestEntry).Loaded = true;
        }

        internal override void DeregisterRegion(RegionRoot region) {
            GetOrLoadRegionData(region.ManifestEntry).Loaded = false;
            base.DeregisterRegion(region);
        }

        private void SaveData_StateChanged() {
            _dirty = true;
        }

        public RegionSaveData GetOrLoadRegionData(LevelManifestRegionEntry region) {
            if (!_loadedRegionData.TryGetValue(region.RegionID, out LoadedRegionInfo info)) {
                info = new LoadedRegionInfo {
                    Data = PersistenceManager.Instance.GetRegionData(LevelIndex, region.RegionID),
                };
                
                info.Data.StateChanged += info.SetDirty;
                _loadedRegionData[region.RegionID] = info;
            }

            return info.Data;
        }

        public void Save() {
            foreach (int regionID in _loadedRegionData.Keys.ToList()) {
                LoadedRegionInfo info = _loadedRegionData[regionID];
                
                if (info.Dirty) {
                    info.Dirty = false;
                    PersistenceManager.Instance.SetRegionData(LevelIndex, regionID, info.Data);
                }

                if (!info.Data.Loaded) {
                    _loadedRegionData.Remove(regionID);
                }
            }
            
            if (_dirty) {
                _dirty = false;
                PersistenceManager.Instance.SetLevelData(LevelIndex, SaveData);
            }
        }
        
        private class LoadedRegionInfo {
            public RegionSaveData Data { get; set; }
            public bool Dirty { get; set; }

            public void SetDirty() => Dirty = true;
        }
    }
}
