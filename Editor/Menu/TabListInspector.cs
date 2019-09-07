using SBR.Menu;
using UnityEditor;
using UnityEngine;

namespace SBR.Editor {
    [CustomEditor(typeof(TabList))]
    public class TabListInspector : UnityEditor.Editor {
        public override void OnInspectorGUI() {
            var list = target as TabList;
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Update UI")) {
                list.UpdateUI();
            }
            if (GUILayout.Button("Clear UI")) {
                list.ClearUI();
            }
            GUILayout.EndHorizontal();

            DrawDefaultInspector();
        }
    }
}