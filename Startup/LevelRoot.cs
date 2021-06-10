using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace SBR.Startup {
    public class LevelRoot : MonoBehaviour {
        [SerializeField] private LevelManifestLevelEntry _manifestEntry;
        public int LevelIndex => _manifestEntry.LevelID;
        public LevelManifestLevelEntry ManifestEntry => _manifestEntry;
        
        private Dictionary<int, bool> _regionsLoaded;

        public virtual void Initialize() {
            
        }
    }
}