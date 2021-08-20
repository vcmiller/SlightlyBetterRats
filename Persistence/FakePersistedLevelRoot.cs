using System;
using System.Collections.Generic;
using System.Linq;

using SBR.Sequencing;

using UnityEngine;

namespace SBR.Persistence {
    public class FakePersistedLevelRoot : MonoBehaviour {
        private void Start() {
            LoadObjects();
        }

        public virtual void LoadObjects() {
            List<PersistedGameObject> gameObjects = new List<PersistedGameObject>();

            PersistedGameObject.CollectGameObjects(gameObject.scene, gameObjects);
            PersistedGameObject.InitializeGameObjects(gameObjects);
        }
    }
}