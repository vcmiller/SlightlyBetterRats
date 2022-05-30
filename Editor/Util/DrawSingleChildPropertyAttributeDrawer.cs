﻿using System;
using System.IO;
using SBR.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SBR {
    [CustomPropertyDrawer(typeof(DrawSingleChildPropertyAttribute))]
    public class DrawSingleChildPropertyAttributeDrawer : PropertyDrawer {
        private DrawSingleChildPropertyAttribute Attribute => (DrawSingleChildPropertyAttribute) attribute;

        private SerializedProperty GetPropertyToDraw(SerializedProperty property) {
            DrawSingleChildPropertyAttribute attr = Attribute;
            SerializedProperty propToDraw = attr != null && !string.IsNullOrEmpty(attr.PropertyName) ? property.FindPropertyRelative(attr.PropertyName) : null;
            propToDraw ??= property;
            return propToDraw;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return EditorGUI.GetPropertyHeight(GetPropertyToDraw(property), label);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            EditorGUI.PropertyField(position, GetPropertyToDraw(property), label);
        }
    }
}