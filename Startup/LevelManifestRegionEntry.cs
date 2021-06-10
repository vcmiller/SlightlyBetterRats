using UnityEngine;

namespace SBR.Startup {
    public class 
        LevelManifestRegionEntry : ScriptableObject {
        [SerializeField] private string _displayName;
        [SerializeField] private int _regionID;
        [SerializeField] private SceneRef _scene;
        
        public string DisplayName => _displayName;
        public int RegionID => _regionID;
        public SceneRef Scene => _scene;
    }
}