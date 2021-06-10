using UnityEngine;

namespace SBR.Startup {
    public class 
        LevelManifestRegionEntry : ScriptableObject {
        [SerializeField] private string _displayName;
        [SerializeField] private int _regionID;
        [SerializeField] private SceneRef _scene;
        [SerializeField] private bool _loadedByDefault;
        
        public string DisplayName => _displayName;
        public int RegionID => _regionID;
        public SceneRef Scene => _scene;
        public bool LoadedByDefault => _loadedByDefault;
    }
}