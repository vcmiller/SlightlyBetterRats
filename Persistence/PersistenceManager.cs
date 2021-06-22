using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

using UnityEngine;

namespace SBR.Persistence {
    public class PersistenceManager : Singleton<PersistenceManager> {
        [SerializeField] private Serializer _serializer;
        [SerializeField] private SaveDataHandler _dataHandler;

        private GlobalSaveData _loadedGlobalData = null;
        private bool _loadedGlobalDataDirty = false;
        public GlobalSaveData LoadedGlobalData {
            get => _loadedGlobalData;
            private set {
                if (_loadedGlobalData == value) return;
                
                if (_loadedGlobalData != null) _loadedGlobalData.StateChanged -= LoadedGlobalData_StateChanged;
                _loadedGlobalData = value;
                if (_loadedGlobalData != null) _loadedGlobalData.StateChanged += LoadedGlobalData_StateChanged;
                
                _loadedGlobalDataDirty = false;
            }
        }

        private ProfileSaveData _loadedProfileData = null;
        private bool _loadedProfileDataDirty = false;

        public ProfileSaveData LoadedProfileData {
            get => _loadedProfileData;
            private set {
                if (_loadedProfileData == value) return;

                if (_loadedProfileData != null) _loadedProfileData.StateChanged -= LoadedProfileData_StateChanged;
                _loadedProfileData = value;
                if (_loadedProfileData != null) _loadedProfileData.StateChanged += LoadedProfileData_StateChanged;

                _loadedProfileDataDirty = false;
            }
        }

        private StateSaveData _loadedStateData = null;
        private bool _loadedStateDataDirty = false;
        public StateSaveData LoadedStateData {
            get => _loadedStateData;
            private set {
                if (_loadedStateData == value) return;

                if (_loadedStateData != null) _loadedStateData.StateChanged -= LoadedStateData_StateChanged;
                _loadedStateData = value;
                if (_loadedStateData != null) _loadedStateData.StateChanged += LoadedStateData_StateChanged;

                _loadedStateDataDirty = false;
            }
        }

        private void LoadedGlobalData_StateChanged() {
            _loadedGlobalDataDirty = true;
        }

        private void LoadedProfileData_StateChanged() {
            _loadedProfileDataDirty = true;
        }

        private void LoadedStateData_StateChanged() {
            _loadedStateDataDirty = true;
        }
        
        public void LoadGlobalData() {
            LoadedGlobalData = _dataHandler.GetGlobalSaveData(_serializer);
            LoadedGlobalData.Initialized = true;
        }

        public void UnloadGlobalData() {
            LoadedGlobalData = null;
            _loadedGlobalDataDirty = false;
            LoadedProfileData = null;
            _loadedProfileDataDirty = false;
            LoadedStateData = null;
            _loadedStateDataDirty = false;
        }

        public void SaveGlobalData() {
            if (!_loadedGlobalDataDirty) return;
            _dataHandler.SetGlobalSaveData(_serializer, _loadedGlobalData);
            _loadedGlobalDataDirty = false;
        }

        public void DeleteGlobalData() {
            _dataHandler.ClearGlobalSaveData();
            UnloadGlobalData();
        }

        public IEnumerable<string> GetAvailableProfiles() {
            return _dataHandler.GetAvailableProfiles();
        }

        public ProfileSaveData GetProfileData(string profile) {
            if (LoadedGlobalData == null) {
                Debug.LogError("Trying to load profile data when global data is not loaded.");
                return null;
            }

            return _dataHandler.GetProfileSaveData(_serializer, profile);
        }

        public void SetProfileData(string profile, ProfileSaveData data) {
            if (LoadedGlobalData == null) {
                Debug.LogError("Trying to save profile data when global data is not loaded.");
                return;
            }

            _dataHandler.SetProfileSaveData(_serializer, profile, data);
        }
        
        public void LoadProfileData(string profile) {
            ProfileSaveData data = GetProfileData(profile);
            if (data == null) return;
            
            LoadedProfileData = _dataHandler.GetProfileSaveData(_serializer, profile);
            LoadedProfileData.Initialized = true;
        }

        public void UnloadProfileData() {
            LoadedProfileData = null;
            _loadedProfileDataDirty = false;
            LoadedStateData = null;
            _loadedStateDataDirty = false;
        }

        public void SaveProfileData() {
            if (!_loadedProfileDataDirty) return;
            _dataHandler.SetProfileSaveData(_serializer, LoadedProfileData.ProfileName, LoadedProfileData);
            _loadedProfileDataDirty = false;
            LoadedGlobalData.MostRecentProfile = LoadedProfileData.ProfileName;
        }

        public void DeleteProfileData(string profile = null) {
            if (string.IsNullOrEmpty(profile)) {
                if (LoadedProfileData != null) {
                    profile = LoadedProfileData.ProfileName;
                } else {
                    Debug.LogError("Trying to delete current profile data when no profile data is loaded.");
                    return;
                }
            }

            bool deletingCurrentProfile = LoadedProfileData != null && LoadedProfileData.ProfileName == profile;
            _dataHandler.ClearProfileSaveData(profile);
            if (deletingCurrentProfile) {
                UnloadProfileData();
            }
        }

        public IEnumerable<int> GetAvailableStates(string profile = null) {
            if (string.IsNullOrEmpty(profile)) {
                if (LoadedProfileData != null) {
                    profile = LoadedProfileData.ProfileName;
                } else {
                    Debug.LogError("Trying to get available states no profile data is loaded.");
                    return Enumerable.Empty<int>();
                }
            }

            return _dataHandler.GetAvailableStates(profile);
        }

        public StateSaveData GetStateData(int stateIndex) {
            if (LoadedProfileData == null) {
                Debug.LogError("Trying to load state data when no profile data is loaded.");
                return null;
            }

            return _dataHandler.GetStateSaveData(_serializer, LoadedProfileData.ProfileName, stateIndex);
        }

        public void SetStateData(int stateIndex, StateSaveData state) {
            if (LoadedProfileData == null) {
                Debug.LogError("Trying to save state data when no profile data is loaded.");
                return;
            }

            _dataHandler.SetStateSaveData(_serializer, LoadedProfileData.ProfileName, stateIndex, state);
        }
        
        public void LoadStateData(int stateIndex) {
            StateSaveData data = GetStateData(stateIndex);
            if (data == null) return;

            LoadedStateData = data;
            LoadedStateData.Initialized = true;
        }

        public void UnloadStateData() {
            LoadedStateData = null;
            _loadedStateDataDirty = false;
        }

        public void SaveStateData() {
            if (!_loadedStateDataDirty) return;
            _dataHandler.SetStateSaveData(_serializer, LoadedProfileData.ProfileName, LoadedStateData.StateIndex,
                                          LoadedStateData);
            _loadedStateDataDirty = false;
            LoadedProfileData.MostRecentState = LoadedStateData.StateIndex;
        }

        public void DeleteStateData(string profile = null, int state = -1) {
            if (string.IsNullOrEmpty(profile)) {
                if (state == -1) {
                    Debug.LogError("Trying to delete current state on a specified profile. This is not allowed.");
                    return;
                } else if (LoadedProfileData == null) {
                    Debug.LogError("Trying to delete current profile state data when no profile data is loaded.");
                    return;
                } else {
                    profile = LoadedProfileData.ProfileName;
                } 
            }

            if (state < 0) {
                if (LoadedStateData == null) {
                    Debug.LogError("Trying to delete current profile state data when no state data is loaded.");
                } else {
                    state = LoadedStateData.StateIndex;
                }
            }
            
            bool deletingCurrentState = LoadedProfileData != null && LoadedProfileData.ProfileName == profile &&
                                        LoadedStateData != null && LoadedStateData.StateIndex == state;
            if (deletingCurrentState) {
                UnloadStateData();
            }
        }

        public LevelSaveData GetLevelData(int levelIndex) {
            if (LoadedStateData == null) {
                Debug.LogError("Trying to load level data when no state data is loaded.");
                return null;
            }

            return _dataHandler.GetLevelSaveData(_serializer, LoadedProfileData.ProfileName, LoadedStateData.StateIndex,
                                                 levelIndex);
        }

        public void SetLevelData(int levelIndex, LevelSaveData level) {
            if (LoadedStateData == null) {
                Debug.LogError("Trying to save level data when no state data is loaded.");
                return;
            }

            _dataHandler.SetLevelSaveData(_serializer, LoadedProfileData.ProfileName, LoadedStateData.StateIndex,
                                          levelIndex, level);
        }

        public RegionSaveData GetRegionData(int levelIndex, int regionIndex) {
            if (LoadedStateData == null) {
                Debug.LogError("Trying to load region data when no state data is loaded.");
                return null;
            }

            return _dataHandler.GetRegionSaveData(_serializer, LoadedProfileData.ProfileName,
                                                  LoadedStateData.StateIndex,
                                                  levelIndex, regionIndex);
        }

        public void SetRegionData(int levelIndex, int regionIndex, RegionSaveData regionData) {
            if (LoadedStateData == null) {
                Debug.LogError("Trying to save region data when no state data is loaded.");
                return;
            }

            _dataHandler.SetRegionSaveData(_serializer, LoadedProfileData.ProfileName,
                                           LoadedStateData.StateIndex,
                                           levelIndex, regionIndex, regionData);
        }
    }
}
