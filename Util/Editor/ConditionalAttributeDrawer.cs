using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SBR.Editor {
    [CustomPropertyDrawer(typeof(ConditionalAttribute))]
    public class ConditionalAttributeDrawer : PropertyDrawer {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            if (ShouldDraw(property)) {
                return EditorGUI.GetPropertyHeight(property, label, true);
            } else {
                return 0;
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            if (ShouldDraw(property)) {
                EditorGUI.PropertyField(position, property, label, true);
            }
        }

        private bool ShouldDraw(SerializedProperty property) {
            SerializedProperty condProperty = null;
            var attr = attribute as ConditionalAttribute;
            
            string propertyPath = property.propertyPath;
            string conditionPath = propertyPath.Replace(property.name, attr.condition);
            condProperty = property.serializedObject.FindProperty(conditionPath);

            if (condProperty == null) {
                condProperty = property.serializedObject.FindProperty(attr.condition);
            }

            if (condProperty == null) {
                Debug.LogError("Could not find property " + attr.condition + " on object " + property.serializedObject.targetObject);
                return true;
            } else {
                var value = condProperty.GetValue();
                return (Equals(value, attr.value) == attr.isEqual);
            }
        }
    }
}
