using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SBR {
    [CustomEditor(typeof(Health))]
    public class HealthInspector : UnityEditor.Editor {
        private static float amt = 20;
        private static readonly string[] exclude = { "m_Script" };

        public override void OnInspectorGUI() {
            EditorGUI.BeginDisabledGroup(!Application.isPlaying);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Damage (" + amt + ")")) {
                var health = target as Health;
                health.Damage(amt);
            }
            
            amt = EditorGUILayout.FloatField(amt);
            EditorGUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();

            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, exclude);
            serializedObject.ApplyModifiedProperties();
        }
    }

}