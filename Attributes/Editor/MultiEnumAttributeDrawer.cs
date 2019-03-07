using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;
using System.Reflection;

namespace SBR.Editor {
    // From Unify Community Wiki
    [CustomPropertyDrawer(typeof(MultiEnumAttribute))]
    public class MultiEnumAttributeDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            MultiEnumAttribute flagSettings = (MultiEnumAttribute)attribute;
            Enum targetEnum = property.FindValue<Enum>();
            
            EditorGUI.BeginProperty(position, label, property);
            Enum enumNew = EditorGUI.EnumFlagsField(position, label, targetEnum);
            property.intValue = (int)Convert.ChangeType(enumNew, targetEnum.GetType());
            EditorGUI.EndProperty();
        }
    }
}
