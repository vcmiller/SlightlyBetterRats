using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using SBR.Sequencing;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace SBR.Persistence {
    public class PersistedGameObject : MonoBehaviour {
        [SerializeField] private int _dynamicPrefabID;
        [SerializeField] private ulong _instanceID;
        [SerializeField] private bool _immediatelyConvert = false;
        
        public ObjectSaveData SaveData { get; private set; }
        public PersistedLevelRoot Level { get; private set; }
        public PersistedRegionRoot Region { get; private set; }

        public int DynamicPrefabID => _dynamicPrefabID;
        
        public bool Initialized { get; private set; }
        
        public bool IsDynamicInstance { get; private set; }

        private IPersistedComponent[] _components;
        private bool _needsToRegister = false;

        private void Awake() {
            _components = GetComponentsInChildren<IPersistedComponent>();
        }

        private PersistedObjectCollection Container {
            get {
                if (Region) return Region.SaveData.Objects;
                if (Level) return Level.SaveData.Objects;
                return null;
            }
        }

        private void OnEnable() {
            _needsToRegister = true;
            IsDynamicInstance = false;
        }

        private void OnDisable() {
            _needsToRegister = false;
            IsDynamicInstance = false;
        }

        private void Update() {
            if (_needsToRegister) {
                Register();
                _needsToRegister = false;
            }
        }

        public void SetupDynamicInstance(ulong instanceID) {
            if (!_needsToRegister) {
                Debug.LogError("SetupDynamicInstance can only be called before a spawned object's first update.");
            }

            _instanceID = instanceID;
            IsDynamicInstance = true;
        }

        private void Register() {
            var scene = gameObject.scene;
            SceneLoadingManager.Instance.GetSceneLoadedState(scene.name, out LevelRoot level, out RegionRoot region);
            if (level is PersistedLevelRoot pLevel) Level = pLevel;
            else if (region is PersistedRegionRoot pRegion) Region = pRegion;
            else {
                Debug.LogError($"Object {name} should be persisted, but is in a scene with no persisted root.");
                return;
            }
            
            SaveData = _instanceID == 0
                ? Container.RegisterNewDynamicObject(_dynamicPrefabID)
                : Container.RegisterExistingObject(_instanceID, IsDynamicInstance);
            _instanceID = SaveData.InstanceID;

            if (SaveData.Destroyed) {
                Spawnable.Despawn(gameObject);
            } else {
                if (!IsDynamicInstance && _immediatelyConvert) {
                    ConvertToDynamicInstance();
                }
                
                InitializeComponents();
            }

            Initialized = true;
        }

        private void InitializeComponents() {
            foreach (IPersistedComponent component in _components) {
                component.Initialize(SaveData, new ComponentID(transform, (Component) component).ToString());
            }
        }

        public void ConvertToDynamicInstance() {
            if (IsDynamicInstance) {
                Debug.LogError($"Trying to convert object {name} with ID {_instanceID} to dynamic, but it already is.");
                return;
            }

            SaveData = Container.ConvertStaticObjectToDynamic(_instanceID, _dynamicPrefabID);
            _instanceID = SaveData.InstanceID;
        }

        public void RegisterDestroyed() {
            Container.RegisterObjectDestroyed(_instanceID);
        }

        public void TransitionToRegion(PersistedRegionRoot newRoot) {
            if (DynamicPrefabID == 0) {
                Debug.LogError($"Trying to transition object {name}, which doesn't have a dynamic prefab ID.");
                return;
            }
            
            RegisterDestroyed();
            Level = null;
            Region = newRoot;
            
            var data = Container.RegisterNewDynamicObject(DynamicPrefabID, _instanceID);
            data.CustomData = SaveData.CustomData;
            data.Destroyed = false;
            data.Initialized = true;
            
            SaveData = data;
            IsDynamicInstance = true;
            _instanceID = SaveData.InstanceID;
            InitializeComponents();
        }
        
        public static void CreateDynamicObjects(PersistedObjectCollection objects, Scene scene) {
            foreach (var objectData in objects.Objects.ToList()) {
                if (!objectData.IsDynamicInstance) continue;
                string path = DynamicObjectManifest.Instance.GetEntry(objectData.DynamicPrefabID)?.ResourcePath;
                if (string.IsNullOrEmpty(path)) {
                    Debug.LogError($"Could not find prefab path for prefab with ID {objectData.DynamicPrefabID}.");
                    continue;
                }

                PersistedGameObject prefab = Resources.Load<PersistedGameObject>(path);
                if (prefab == null) {
                    Debug.LogError($"Could not load prefab at path {path}.");
                    continue;
                }

                Spawnable.Spawn(prefab, persistedInstanceID: objectData.InstanceID, scene: scene);
            }
        }
    }
}
