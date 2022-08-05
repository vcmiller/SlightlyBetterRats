using UnityEditor;
using UnityEngine;

namespace SBR.Editor {
    [CustomEditor(typeof(SwappableMesh)), CanEditMultipleObjects]
    public class SwappableMeshEditor : UnityEditor.Editor {
        public override void OnInspectorGUI() {
            DrawDefaultInspector();
            if (GUILayout.Button("Refresh")) {
                foreach (Object t in targets) {
                    ((SwappableMesh)t).RefreshModel();
                }
            }
        }
    }
}