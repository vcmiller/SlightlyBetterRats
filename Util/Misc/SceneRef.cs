using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SBR {
    [Serializable]
    public struct SceneRef {
        [SerializeField] private string _path;

        public string Path => _path;
        public string Name => System.IO.Path.GetFileNameWithoutExtension(Path);
    }
}
