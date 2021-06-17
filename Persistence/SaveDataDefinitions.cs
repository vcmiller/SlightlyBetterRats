using System;
using System.Collections.Generic;

namespace SBR.Persistence {
    public class PersistedDataBase {
        public event Action StateChanged;

        public void NotifyStateChanged() => StateChanged?.Invoke();
    }
    
    [Serializable]
    public class GlobalSaveData : PersistedDataBase {
        private object _customGlobalData;
        private bool _initialized;

        public object CustomGlobalData {
            get => _customGlobalData;
            set {
                if (_customGlobalData == value) return;
                _customGlobalData = value;
                NotifyStateChanged();
            }
        }

        public bool Initialized {
            get => _initialized;
            set {
                if (_initialized == value) return;
                _initialized = value;
                NotifyStateChanged();
            }
        }
    }

    [Serializable]
    public class ProfileSaveData : PersistedDataBase {
        private string _profileName;
        private object _customProfileData;
        private bool _initialized;

        public string ProfileName {
            get => _profileName;
            set {
                if (_profileName == value) return;
                _profileName = value;
                NotifyStateChanged();
            }
        }

        public object CustomProfileData {
            get => _customProfileData;
            set {
                if (_customProfileData == value) return;
                _customProfileData = value;
                NotifyStateChanged();
            }
        }

        public bool Initialized {
            get => _initialized;
            set {
                if (_initialized == value) return;
                _initialized = value;
                NotifyStateChanged();
            }
        }
    }

    [Serializable]
    public class StateSaveData : PersistedDataBase {
        private int _stateIndex;
        private object _customStateData;
        private bool _initialized;
        private string _currentScene;

        public int StateIndex {
            get => _stateIndex;
            set {
                if (_stateIndex == value) return;
                _stateIndex = value;
                return;
            }
        }

        public object CustomStateData {
            get => _customStateData;
            set {
                if (_customStateData == value) return;
                _customStateData = value;
                NotifyStateChanged();
            }
        }

        public bool Initialized {
            get => _initialized;
            set {
                if (_initialized == value) return;
                _initialized = value;
                NotifyStateChanged();
            }
        }

        public string CurrentScene {
            get => _currentScene;
            set {
                if (_currentScene == value) return;
                _currentScene = value;
                NotifyStateChanged();
            }
        }
    }

    [Serializable]
    public class LevelSaveData : PersistedDataBase {
        private int _levelIndex;
        private object _customLevelData;
        private bool _initialized;

        public int LevelIndex {
            get => _levelIndex;
            set {
                if (_levelIndex == value) return;
                _levelIndex = value;
                NotifyStateChanged();
            }
        }

        public object CustomLeveData {
            get => _customLevelData;
            set {
                if (_customLevelData == value) return;
                _customLevelData = value;
                NotifyStateChanged();
            }
        }

        public bool Initialized {
            get => _initialized;
            set {
                if (_initialized == value) return;
                _initialized = value;
                NotifyStateChanged();
            }
        }
    }

    [Serializable]
    public class RegionSaveData : PersistedDataBase {
        private int _regionIndex;
        private Dictionary<int, ObjectSaveData> _objects = new Dictionary<int, ObjectSaveData>();
        private object _customRegionData;
        private bool _initialized;
        private bool _loaded;
        
        public int RegionIndex {
            get => _regionIndex;
            set {
                if (_regionIndex == value) return;
                _regionIndex = value;
                NotifyStateChanged();
            }
        }

        public IReadOnlyDictionary<int, ObjectSaveData> Objects => _objects;
        
        public object CustomRegionData {
            get => _customRegionData;
            set {
                if (_customRegionData == value) return;
                _customRegionData = value;
                NotifyStateChanged();
            }
        }

        public bool Initialized {
            get => _initialized;
            set {
                if (_initialized == value) return;
                _initialized = value;
                NotifyStateChanged();
            }
        }

        public bool Loaded {
            get => _loaded;
            set {
                if (_loaded == value) return;
                _loaded = value;
                NotifyStateChanged();
            }
        }
    }

    [Serializable]
    public class ObjectSaveData : PersistedDataBase {
        private int _objectID;
        private float _px, _py, _pz;
        private float _rx, _ry, _rz, _rw;
        private bool _isDynamicInstance;
        private int _dynamicPrefabId;
        private object _customObjectData;
        private bool _initialized;
    }
}
