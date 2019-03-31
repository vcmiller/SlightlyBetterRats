﻿using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace SBR.Editor {
    [CustomPropertyDrawer(typeof(Internal.DraggableList), true)]
    public class DraggableListDrawer : PropertyDrawer {
        private Dictionary<string, ReorderableList> lists =
            new Dictionary<string, ReorderableList>();
        private static GUIStyle bgStyle;

        protected virtual bool showExpanders => true;
        protected virtual string labelProperty => null;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            if (property.isExpanded) {
                return GetList(property).GetHeight();
            } else {
                return 16;
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            if (property.isExpanded) {
                var list = GetList(property);
                list.DoList(position);
            } else {
                property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, label);
            }
        }

        private float GetElementHeight(SerializedProperty prop, int index) {
            SerializedProperty arrayElement = prop.GetArrayElementAtIndex(index);
            float calculatedHeight = EditorGUI.GetPropertyHeight(arrayElement,
                                                                GUIContent.none,
                                                                arrayElement.isExpanded || !showExpanders);
            calculatedHeight += 3;
            return calculatedHeight;
        }

        private ReorderableList GetList(SerializedProperty prop) {
            string key = prop.serializedObject.GetHashCode() + prop.propertyPath;
            if (!lists.ContainsKey(key)) {
                var listProp = prop.FindPropertyRelative("items");
                lists[key] = new ReorderableList(prop.serializedObject, listProp, true, true, true, true);

                lists[key].drawHeaderCallback = (Rect rect) => {
                    rect.xMin += 10;
                    prop.isExpanded = EditorGUI.Foldout(rect, prop.isExpanded, prop.displayName);
                };

                lists[key].drawElementCallback =
                    (Rect rect, int index, bool isActive, bool isFocused) => {
                        var childProp = listProp.GetArrayElementAtIndex(index);
                        bool isExpanded = childProp.isExpanded || !showExpanders;
                        rect.height = EditorGUI.GetPropertyHeight(childProp, GUIContent.none, isExpanded);

                        if (childProp.hasVisibleChildren)
                            rect.xMin += 10;
                        
                        GUIContent propHeader = new GUIContent(childProp.displayName);
                        if (!string.IsNullOrEmpty(labelProperty)) {
                            var dispProp = childProp.FindPropertyRelative(labelProperty);
                            if (dispProp != null) {
                                if (dispProp.propertyType == SerializedPropertyType.String) {
                                    if (!string.IsNullOrEmpty(dispProp.stringValue)) {
                                        propHeader.text = dispProp.stringValue;
                                    }
                                } else if (dispProp.propertyType == SerializedPropertyType.ObjectReference) {
                                    if (dispProp.objectReferenceValue) {
                                        propHeader.text = dispProp.objectReferenceValue.name;
                                    }
                                }
                            }
                        }
                        EditorGUI.PropertyField(rect, childProp, propHeader, isExpanded);
                    };

                lists[key].elementHeightCallback = index => GetElementHeight(listProp, index);

                lists[key].drawElementBackgroundCallback = (rect, index, active, focused) => {
                    if (bgStyle == null)
                        bgStyle = GUI.skin.FindStyle("MeTransitionSelectHead");
                    if (focused == false)
                        return;
                    rect.height = GetElementHeight(listProp, index);
                    GUI.Box(rect, GUIContent.none, bgStyle);
                };
            }

            return lists[key];
        }
    }

    [CustomPropertyDrawer(typeof(DraggableListDisplayAttribute))]
    public class DraggableListDisplayDrawer : DraggableListDrawer {
        private DraggableListDisplayAttribute attr => attribute as DraggableListDisplayAttribute;

        protected override string labelProperty => attr.labelProperty;
        protected override bool showExpanders => attr.showExpanders;
    }
}