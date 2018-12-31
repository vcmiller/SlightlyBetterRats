using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using SBR.StateMachines;
using System.Linq;
using System;

namespace SBR.Editor {
    [CustomPropertyDrawer(typeof(StateSelectAttribute))]
    public class StateSelectAttributeDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            if (property.propertyType != SerializedPropertyType.String) {
                EditorGUI.PropertyField(position, property, label);
            } else {
                StateMachineDefinition sm = property.serializedObject.targetObject as StateMachineDefinition;
                GUIContent[] options = sm.states.Select(s => new GUIContent(s.name)).Prepend(new GUIContent("(none)")).ToArray();
                int index = Array.FindIndex(options, c => c.text == property.stringValue);
                index = EditorGUI.Popup(position, label, index, options);

                if (index > 0) {
                    property.stringValue = options[index].text;
                } else {
                    property.stringValue = "";
                }
            }
        }
    }
}
