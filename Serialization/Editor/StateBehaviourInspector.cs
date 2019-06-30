using SBR.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SBR.Editor {

    [CustomEditor(typeof(StateBehaviour), true)]
    public class StateBehaviourInspector : UnityEditor.Editor {
        public override void OnInspectorGUI() {
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, "m_Script", "states");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("states"));
            serializedObject.ApplyModifiedProperties();
        }
    }

}