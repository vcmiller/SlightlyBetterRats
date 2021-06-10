using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SBR.Startup {
    public class LevelRoot : MonoBehaviour {
        [SerializeField] private int _levelIndex;
        [SerializeField] private RegionInfo[] _regionScenes;

        public int LevelIndex => _levelIndex;
        public IReadOnlyList<RegionInfo> RegionScenes => _regionScenes;
        
        private bool[] _regionsLoaded;

        public virtual IEnumerable<AsyncOperation> Initialize(bool autoActivate) {
            return LoadInitialRegions(autoActivate);
        }

        protected virtual IEnumerable<AsyncOperation> LoadInitialRegions(bool autoActivate) {
            for (int i = 0; i < _regionScenes.Length; i++) {
                AsyncOperation op = SetRegionLoaded(i, _regionScenes[i].DefaultLoaded, autoActivate);
                if (op != null) yield return op;
            }
        }
        
        public virtual bool IsRegionLoaded(int regionIndex) {
            if (_regionsLoaded == null || _regionsLoaded.Length <= regionIndex) return false;
            return _regionsLoaded[regionIndex];
        }
        
        public virtual AsyncOperation SetRegionLoaded(int regionIndex, bool loaded, bool autoActivate) {
            if (IsRegionLoaded(regionIndex) == loaded) return null;
            if (regionIndex < 0 || regionIndex >= _regionScenes.Length || string.IsNullOrEmpty(_regionScenes[regionIndex].SceneName)) {
                Debug.LogError($"Invalid region index {regionIndex} for level {_levelIndex}.");
                return null;
            }

            AsyncOperation op = loaded
                ? SceneManager.LoadSceneAsync(_regionScenes[regionIndex].SceneName, LoadSceneMode.Additive)
                : SceneManager.UnloadSceneAsync(_regionScenes[regionIndex].SceneName);

            if (op == null) return null;
            
            op.allowSceneActivation = autoActivate;
            return op;
        }
        
        [Serializable]
        public class RegionInfo {
            [SerializeField] private SceneRef _scene;
            [SerializeField] private bool _defaultLoaded;

            public string SceneName => _scene.Name;
            public string ScenePath => _scene.Path;
            public bool DefaultLoaded => _defaultLoaded;
        }
    }
}