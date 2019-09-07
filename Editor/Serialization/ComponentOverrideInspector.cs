using SBR.Serialization;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace SBR.Editor {
    [CustomEditor(typeof(ComponentOverride))]
    public class ComponentOverrideInspector : UnityEditor.Editor {
        private new ComponentOverride target => base.target as ComponentOverride;
        private static List<string> headers = new List<string>();

        private void ListDropdown() {
            var menu = new GenericMenu();
            
            foreach (var path in target.uniqueValidPaths) {
                menu.AddItem(new GUIContent(path),
                    false, (object data) => {
                        Undo.RecordObject(target, "Add Override");
                        target.AddOverride(path);
                        EditorUtility.SetDirty(target);
                    },
                    null);
            }

            menu.ShowAsContext();
        }

        private void RemoveOverride(int index) {
            Undo.RecordObject(target, "Remove Override");
            target.RemoveOverride(index);
            EditorUtility.SetDirty(target);
        }

        public override void OnInspectorGUI() {
            target.CheckValid();

            serializedObject.Update();
            var type = target.typeName;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("typeName"), new GUIContent("Target Type"));
            serializedObject.ApplyModifiedProperties();

            if (target.typeName != type) {
                target.NotifyChanged();
            }

            if (target.type == null) {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Invalid Target Type", EditorStyles.boldLabel);
            } else {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Value Overrides", EditorStyles.boldLabel);
                int toRemove = -1;
                headers.Clear();
                var indent = EditorGUI.indentLevel;
                for (int i = 0; i < target.overrides.Length; i++) {
                    var prop = target.overrides[i];
                    SerializedValueDrawer.UpdateHeading(prop, headers);
                    EditorGUILayout.BeginHorizontal();
                    SerializedValueDrawer.DrawLayout(target, prop);
                    if (GUILayout.Button("X", GUILayout.MaxWidth(20), GUILayout.MinWidth(20))) {
                        toRemove = i;
                    }
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUI.BeginDisabledGroup(!target.uniqueValidPaths.Any());
                EditorGUI.indentLevel = indent;
                EditorGUILayout.Space();
                if (EditorGUILayout.DropdownButton(new GUIContent("Add Override"), FocusType.Keyboard, GUILayout.MaxWidth(150))) {
                    ListDropdown();
                }
                EditorGUI.EndDisabledGroup();

                if (toRemove >= 0) {
                    RemoveOverride(toRemove);
                }
            }
        }
    }
}
