using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SBR {
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour {
        private static T _instance;

        public static T Instance {
            get {
                if (_instance == null) {
                    _instance = FindObjectOfType<T>();
                    if (_instance == null) {
                        Debug.LogError($"{typeof(T)} instance not found!");
                    }
                }

                return _instance;
            }
        }
    }
}