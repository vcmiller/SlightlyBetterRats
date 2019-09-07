using SBR.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace SBR.Editor {
    [CustomPropertyDrawer(typeof(StateList))]
    public class StateListDrawer : DraggableListDrawer {
        const int gap = 4;
        private string[] stateNames;

        protected override ReorderableList CreateList(SerializedProperty prop) {
            var list = base.CreateList(prop);
            list.elementHeightCallback = null;
            list.onAddDropdownCallback = ListDropdown;
            return list;
        }

        protected override SerializedProperty GetListProperty(SerializedProperty prop) {
            return prop.FindPropertyRelative("states");
        }

        private static Type GetStateType(SerializedProperty property) {
            var target = property.serializedObject.targetObject;
            if (target is IStateBehaviour stateBehaviour) {
                return stateBehaviour.type;
            } else {
                return target.GetType();
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            if (property.isExpanded) {
                var type = GetStateType(property);
                var listProp = GetListProperty(property);
                List<string> names = new List<string>();
                for (int i = 0; i < listProp.arraySize; i++) {
                    var nameProperty = GetElementProperty(property, i).FindPropertyRelative("name");

                    string suffix = "";
                    int num = 0;
                    while (names.Contains(nameProperty.stringValue + suffix)) {
                        num++;
                        suffix = "_" + num;
                    }
                    if (num > 0) nameProperty.stringValue += suffix;
                    names.Add(nameProperty.stringValue);

                    var valuesProperty = GetElementProperty(property, i).FindPropertyRelative("values");
                    var values = valuesProperty.objectReferenceValue as ComponentOverride;
                    if (values && !values.type.IsAssignableFrom(type)) {
                        valuesProperty.objectReferenceValue = null;
                    }
                }
                stateNames = names.ToArray();
            }

            base.OnGUI(position, property, label);
        }

        private SerializedProperty CreateNewState(SerializedProperty listProperty, string name, ComponentOverride values) {
            listProperty.serializedObject.Update();
            Undo.RecordObject(listProperty.serializedObject.targetObject, "Create State");
            listProperty.arraySize++;

            var newState = listProperty.GetArrayElementAtIndex(listProperty.arraySize - 1);
            newState.FindPropertyRelative("name").stringValue = name;
            newState.FindPropertyRelative("values").objectReferenceValue = values;
            newState.FindPropertyRelative("blockedBy").intValue = 0;
            newState.FindPropertyRelative("isActive").boolValue = false;
            listProperty.serializedObject.ApplyModifiedProperties();

            return newState;
        }

        private void ListDropdown(Rect rect, ReorderableList list) {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Use Existing Override"),
                false, data => {
                    CreateNewState(list.serializedProperty, "Empty", null);
                }, null);

            menu.AddItem(new GUIContent("Create New Override"),
                false, data => {
                    var type = GetStateType(list.serializedProperty);
                    string path = EditorUtility.SaveFilePanelInProject("Save New Override", ComponentOverride.DefaultNameForType(type), "asset", "Create");
                    if (!string.IsNullOrEmpty(path)) {
                        var newOverride = ScriptableObject.CreateInstance<ComponentOverride>();
                        newOverride.typeName = type.FullName;
                        AssetDatabase.CreateAsset(newOverride, path);
                        CreateNewState(list.serializedProperty, newOverride.displayName, newOverride);
                    }
                }, null);

            menu.ShowAsContext();
        }

        protected override void DrawElement(Rect rect, int index, bool isActive, bool isFocused, SerializedProperty prop) {
            var element = GetElementProperty(prop, index);
            rect.height = EditorGUI.GetPropertyHeight(element.FindPropertyRelative("name"), GUIContent.none);

            Util.SplitHorizontal(rect, gap, out var r1, out var r2);
            Util.SplitHorizontal(r1, gap, out var r3, out var r4);
            Util.SplitHorizontal(r2, gap, out var r5, out var r6, 0.4f);

            EditorGUI.PropertyField(r3, element.FindPropertyRelative("name"), GUIContent.none);
            EditorGUI.PropertyField(r4, element.FindPropertyRelative("values"), GUIContent.none);

            var align = GUI.skin.label.alignment;
            GUI.skin.label.alignment = TextAnchor.MiddleRight;

            r5.xMin += Mathf.Max(0, r5.width - 75);
            EditorGUI.LabelField(r5, new GUIContent("Blocked By:"));
            GUI.skin.label.alignment = align;

            r6.xMax -= r6.height + gap;
            var r7 = new Rect(r6.xMax + gap, r6.y, r6.height, r6.height);

            if (Application.isPlaying && prop.serializedObject.targetObject is IStateBehaviour stateBehavior) {
                var name = element.FindPropertyRelative("name").stringValue;
                bool active = stateBehavior.IsStateActive(name);
                bool newActive = EditorGUI.Toggle(r7, active);
                if (newActive != active) {
                    stateBehavior.SetStateActive(name, newActive);
                }
            } else {
                EditorGUI.PropertyField(r7, element.FindPropertyRelative("isActive"), GUIContent.none);
            }

            var maskProp = element.FindPropertyRelative("blockedBy");
            maskProp.intValue = EditorGUI.MaskField(r6, maskProp.intValue, stateNames) & ~(1 << index);
        }
    }
}