using UnityEditor;
using UnityEngine;

namespace SBR.Editor {
    public static class SceneCameraRollFix {
        [MenuItem("Tools/Infohazard/Fix Scene Camera Roll")]
        public static void FixSceneCameraRoll() {
            SceneView sceneView = SceneView.lastActiveSceneView;
            Vector3 angles = sceneView.rotation.eulerAngles;
            angles.z = 0;
            sceneView.rotation = Quaternion.Euler(angles);
        }
    }
}
