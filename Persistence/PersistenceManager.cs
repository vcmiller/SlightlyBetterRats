using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

using UnityEngine;

namespace SBR.Persistence {
    public class PersistenceManager : MonoBehaviour {
        [SerializeField] private Serializer _serializer;
        [SerializeField] private SaveDataHandler _dataHandler;

        private static PersistenceManager _instance;
        public static PersistenceManager Instance {
            get {
                if (_instance == null) {
                    _instance = FindObjectOfType<PersistenceManager>();
                    if (_instance == null) {
                        Debug.LogError("No persistence manager!");
                        SceneControl.Quit();
                    }
                }

                return _instance;
            }
        }

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

        public void SaveGlobalData() {
            if (!_loadedGlobalDataDirty) return;
            _dataHandler.SetGlobalSaveData(_serializer, _loadedGlobalData);
            _loadedGlobalDataDirty = false;
        }
        
        public void LoadProfileData(string profile) {
            if (LoadedGlobalData == null) {
                Debug.LogError("Trying to load profile data when global data is not loaded.");
                return;
            }

            LoadedProfileData = _dataHandler.GetProfileSaveData(_serializer, profile);
            LoadedProfileData.Initialized = true;
        }

        public void SaveProfileData() {
            if (!_loadedProfileDataDirty) return;
            _dataHandler.SetProfileSaveData(_serializer, LoadedProfileData.ProfileName, LoadedProfileData);
            _loadedProfileDataDirty = false;
        }
        
        public void LoadStateData(int stateIndex) {
            if (LoadedProfileData == null) {
                Debug.LogError("Trying to load state data when no profile data is loaded.");
                return;
            }

            LoadedStateData = _dataHandler.GetStateSaveData(_serializer, LoadedProfileData.ProfileName, stateIndex);
            LoadedStateData.Initialized = true;
        }

        public void SaveStateData() {
            if (!_loadedStateDataDirty) return;
            _dataHandler.SetStateSaveData(_serializer, LoadedProfileData.ProfileName, LoadedStateData.StateIndex,
                                          LoadedStateData);
            _loadedStateDataDirty = false;
        }

        public LevelSaveData GetLevelData(int levelIndex) {
            if (LoadedStateData == null) {
                Debug.LogError("Trying to load level data when no state data is loaded.");
                return null;
            }

            return _dataHandler.GetLevelSaveData(_serializer, LoadedProfileData.ProfileName, LoadedStateData.StateIndex,
                                                 levelIndex);
        }

        public RegionSaveData GetRegionData(int levelIndex, int regionIndex) {
            if (LoadedStateData == null) {
                Debug.LogError("Trying to load level data when no state data is loaded.");
                return null;
            }

            return _dataHandler.GetRegionSaveData(_serializer, LoadedProfileData.ProfileName,
                                                  LoadedStateData.StateIndex,
                                                  levelIndex, regionIndex);
        }
    }
}
