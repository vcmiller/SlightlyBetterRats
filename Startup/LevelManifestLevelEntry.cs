using System.Collections.Generic;

using UnityEngine;

namespace SBR.Startup {
    public class LevelManifestLevelEntry : ScriptableObject {
        [SerializeField] private string _displayName;
        [SerializeField] private int _levelID;
        [SerializeField] private SceneRef _scene;

        [SerializeField] [Expandable(false, typeof(LevelManifestRegionEntry))]
        private LevelManifestRegionEntry[] _regions;

        public string DisplayName => _displayName;
        public int LevelID => _levelID;
        public SceneRef Scene => _scene;
        public IReadOnlyList<LevelManifestRegionEntry> Regions => _regions;
    }
}