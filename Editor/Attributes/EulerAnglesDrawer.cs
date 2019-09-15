using UnityEditor;
using UnityEngine;

namespace SBR.Editor {
    [CustomPropertyDrawer(typeof(EulerAnglesAttribute))]
    public class EulerAnglesDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            Vector3 euler = property.quaternionValue.eulerAngles;
            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginChangeCheck();
            euler = EditorGUI.Vector3Field(position, label, euler);
            if (EditorGUI.EndChangeCheck())
                property.quaternionValue = Quaternion.Euler(euler);
            EditorGUI.EndProperty();
        }
    }
}