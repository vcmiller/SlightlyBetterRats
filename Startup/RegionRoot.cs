using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SBR.Startup {
    public class RegionRoot : MonoBehaviour {
        [SerializeField] private int _levelIndex;
        [SerializeField] private int _regionIndex;

        public int LevelIndex => _levelIndex;
        public int RegionIndex => _regionIndex;
    }
}