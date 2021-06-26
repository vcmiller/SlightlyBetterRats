using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using Random = System.Random;

namespace SBR.Persistence {
    [Serializable]
    public class PersistedDataBase {
        private bool _initialized;
        public bool Initialized {
            get => _initialized;
            set {
                if (_initialized == value) return;
                _initialized = value;
                NotifyStateChanged();
            }
        }


        [NonSerialized] private bool _subscribed;
        protected bool Subscribed {
            get => _subscribed;
            private set {
                if (value == _subscribed) return;
                _subscribed = value;
                if (value) Subscribe();
                else Unsubscribe();
            }
        }

        private Dictionary<string, object> _customData;
        public IReadOnlyDictionary<string, object> CustomData {
            get => _customData;
            set {
                if (Equals(_customData, value)) return;
                bool b = Subscribed;
                Subscribed = false;
                _customData = value.ToDictionary(pair => pair.Key, pair => pair.Value);
                Subscribed = b;
            }
        }

        [NonSerialized] private Action _stateChanged;
        public bool HasStateChangedListeners => _stateChanged != null;
        public event Action StateChanged {
            add {
                _stateChanged += value;
                Subscribed = HasStateChangedListeners;
            }
            remove {
                _stateChanged -= value;
                Subscribed = HasStateChangedListeners;
            }
        }

        public void NotifyStateChanged() => _stateChanged?.Invoke();

        protected virtual void Subscribe() {
            if (_customData == null) return;
            foreach (object obj in _customData.Values) {
                if (!(obj is PersistedDataBase data)) continue;
                data.StateChanged -= CustomData_StateChanged;
            }
        }

        protected virtual void Unsubscribe() {
            if (_customData == null) return;
            foreach (object obj in _customData.Values) {
                if (!(obj is PersistedDataBase data)) continue;
                data.StateChanged += CustomData_StateChanged;
            }
        }

        public object GetCustomData(string key) {
            _customData ??= new Dictionary<string, object>();
            return _customData.TryGetValue(key, out object value) ? value : null;
        }

        public void SetCustomData(string key, object value) {
            _customData ??= new Dictionary<string, object>();

            if (_customData.TryGetValue(key, out object prevValue) && prevValue is PersistedDataBase prevData) {
                prevData.StateChanged -= CustomData_StateChanged;
            }
            
            _customData[key] = value;
            
            if (value is PersistedDataBase data) {
                data.StateChanged += CustomData_StateChanged;
            }
        }

        private void CustomData_StateChanged() {
            NotifyStateChanged();
        }
    }
    
    [Serializable]
    public class GlobalSaveData : PersistedDataBase {
        private string _mostRecentProfile;

        public string MostRecentProfile {
            get => _mostRecentProfile;
            set {
                if (_mostRecentProfile == value) return;
                _mostRecentProfile = value;
                NotifyStateChanged();
            }
        }
    }

    [Serializable]
    public class ProfileSaveData : PersistedDataBase {
        private string _profileName;
        private string _mostRecentState;

        public string ProfileName {
            get => _profileName;
            set {
                if (_profileName == value) return;
                _profileName = value;
                NotifyStateChanged();
            }
        }

        public string MostRecentState {
            get => _mostRecentState;
            set {
                if (_mostRecentState == value) return;
                _mostRecentState = value;
                NotifyStateChanged();
            }
        }
    }

    [Serializable]
    public class StateSaveData : PersistedDataBase {
        private string _stateName;
        private string _currentScene;

        public string StateName {
            get => _stateName;
            set {
                if (_stateName == value) return;
                _stateName = value;
                return;
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
    public class ObjectContainerSaveDataBase : PersistedDataBase {
        private PersistedObjectCollection _objects = new PersistedObjectCollection();
        public PersistedObjectCollection Objects => _objects;

        protected override void Subscribe() {
            base.Subscribe();
            Objects.StateChanged += Objects_StateChanged;
        }

        protected override void Unsubscribe() {
            base.Subscribe();
            Objects.StateChanged -= Objects_StateChanged;
        }

        private void Objects_StateChanged() {
            NotifyStateChanged();
        }
    }

    [Serializable]
    public class LevelSaveData : ObjectContainerSaveDataBase {
        private int _levelIndex;

        public int LevelIndex {
            get => _levelIndex;
            set {
                if (_levelIndex == value) return;
                _levelIndex = value;
                NotifyStateChanged();
            }
        }
    }

    [Serializable]
    public class RegionSaveData : ObjectContainerSaveDataBase {
        private int _regionIndex;
        private bool _loaded;
        
        public int RegionIndex {
            get => _regionIndex;
            set {
                if (_regionIndex == value) return;
                _regionIndex = value;
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
    public class PersistedObjectCollection : PersistedDataBase {
        private Dictionary<ulong, ObjectSaveData> _objects = new Dictionary<ulong, ObjectSaveData>();

        private static readonly Random LongRandomizer = new Random();

        public IEnumerable<ObjectSaveData> Objects => _objects.Values;


        private ulong GetUnusedInstanceID() {
            ulong id;
            do {
                id = LongRandomizer.NextUlong();
            } while (id == 0 || _objects.ContainsKey(id));

            return id;
        }

        public ObjectSaveData RegisterExistingObject(ulong instanceID, bool isDynamic) {
            if (!_objects.TryGetValue(instanceID, out ObjectSaveData data)) {
                data = new ObjectSaveData(instanceID, isDynamic, -1);
                _objects[instanceID] = data;
                Subscribe(data);
                NotifyStateChanged();
            }

            return data;
        }

        public ObjectSaveData RegisterNewDynamicObject(int prefabID) {
            ulong instanceID = GetUnusedInstanceID();
            ObjectSaveData data = new ObjectSaveData(instanceID, true, prefabID);
            _objects[instanceID] = data;
            Subscribe(data);
            NotifyStateChanged();
            return data;
        }

        public ObjectSaveData ConvertStaticObjectToDynamic(ulong instanceID, int prefabID) {
            ObjectSaveData staticData = RegisterExistingObject(instanceID, false);
            staticData.Destroyed = true;
            ulong newInstanceID = GetUnusedInstanceID();
            ObjectSaveData instanceData = new ObjectSaveData(newInstanceID, true, prefabID) {
                CustomData = staticData.CustomData,
                Destroyed = false,
                Initialized = true,
            };
            _objects[newInstanceID] = instanceData;
            Subscribe(instanceData);
            NotifyStateChanged();
            return instanceData;
        }

        public void RegisterObjectDestroyed(ulong instanceID) {
            if (!_objects.TryGetValue(instanceID, out ObjectSaveData data)) {
                Debug.LogError($"Invalid instanceID {instanceID} passed to RegisterObjectDestroyed.");
                return;
            }

            if (data.IsDynamicInstance) {
                _objects.Remove(instanceID);
                Unsubscribe(data);
            } else {
                data.Destroyed = true;
            }
            NotifyStateChanged();
        }

        protected override void Subscribe() {
            base.Subscribe();
            foreach (var data in _objects.Values) {
                Subscribe(data);
            }
        }

        protected override void Unsubscribe() {
            base.Unsubscribe();
            foreach (var data in _objects.Values) {
                Unsubscribe(data);
            }
        }

        protected void Subscribe(ObjectSaveData data) {
            if (Subscribed) data.StateChanged += ObjectData_StateChanged;
        }

        private void Unsubscribe(ObjectSaveData data) {
            data.StateChanged -= ObjectData_StateChanged;
        }

        private void ObjectData_StateChanged() {
            NotifyStateChanged();
        }
    }

    [Serializable]
    public class ObjectSaveData : PersistedDataBase {
        private ulong _instanceID;
        private bool _isDynamicInstance;
        private int _dynamicPrefabID;
        private bool _destroyed;

        public ulong InstanceID => _instanceID;

        public bool IsDynamicInstance => _isDynamicInstance;

        public int DynamicPrefabID => _dynamicPrefabID;
        
        public ObjectSaveData() {}

        public ObjectSaveData(ulong instanceID, bool isDynamicInstance, int dynamicPrefabID) {
            _instanceID = instanceID;
            _isDynamicInstance = isDynamicInstance;
            _dynamicPrefabID = dynamicPrefabID;
        }

        public bool Destroyed {
            get => _destroyed;
            set {
                if (_destroyed == value) return;
                _destroyed = value;
                NotifyStateChanged();
            }
        }
    }
}
