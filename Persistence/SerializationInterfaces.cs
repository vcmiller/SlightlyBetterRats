using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEngine;

namespace SBR.Persistence {
    public abstract class Serializer : MonoBehaviour {
        public abstract void Write(Stream stream, object data);
        public abstract bool Read<T>(Stream stream, out T data);
    }

    public abstract class SaveDataHandler : MonoBehaviour {
        public abstract GlobalSaveData GetGlobalSaveData(Serializer serializer);
        public abstract void SetGlobalSaveData(Serializer serializer, GlobalSaveData globalData);
        public abstract void ClearGlobalSaveData();
        
        public abstract IEnumerable<string> GetAvailableProfiles();
        public abstract ProfileSaveData GetProfileSaveData(Serializer serializer, string profile);
        public abstract void SetProfileSaveData(Serializer serializer, string profile, ProfileSaveData profileData);
        public abstract void ClearProfileSaveData(string profile);
        
        public abstract IEnumerable<int> GetAvailableStates(string profile);
        public abstract StateSaveData GetStateSaveData(Serializer serializer, string profile, int state);
        public abstract void SetStateSaveData(Serializer serializer, string profile, int state, StateSaveData stateData);
        public abstract void ClearStateSaveData(string profile, int state);
        
        public abstract IEnumerable<int> GetAvailableLevels(string profile, int state);
        public abstract LevelSaveData GetLevelSaveData(Serializer serializer, string profile, int state, int level);
        public abstract void SetLevelSaveData(Serializer serializer, string profile, int state, int level, LevelSaveData levelData);
        public abstract void ClearLevelSaveData(string profile, int state, int level);
        
        public abstract IEnumerable<int> GetAvailableRegions(string profile, int state, int level);
        public abstract RegionSaveData GetRegionSaveData(Serializer serializer, string profile, int state, int level, int region);
        public abstract void SetRegionSaveData(Serializer serializer, string profile, int state, int level, int region, RegionSaveData regionData);
        public abstract void ClearRegionSaveData(string profile, int state, int level, int region);
    }
}

