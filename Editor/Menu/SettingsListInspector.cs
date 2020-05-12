// MIT License
// 
// Copyright (c) 2020 Vincent Miller
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using SBR.Menu;
using UnityEditor;
using UnityEngine;
using UnityEditorInternal;

namespace SBR.Editor {
    [CustomEditor(typeof(SettingsList))]
    public class SettingsListInspector : UnityEditor.Editor {
        private ReorderableList list;

        private void OnEnable() {
            list = null;
        }

        public override void OnInspectorGUI() {
            var list = target as SettingsList;
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Update UI")) {
                list.UpdateUI();
            }
            if (GUILayout.Button("Clear UI")) {
                list.ClearUI();
            }
            GUILayout.EndHorizontal();

            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, "m_Script", "settings");
            ShowList();
            serializedObject.ApplyModifiedProperties();
        }

        private void SetupList() {
            list = new ReorderableList(serializedObject,
                   serializedObject.FindProperty("settings"), true, true, true, true);

            list.drawHeaderCallback = (Rect rect) => {
                EditorGUI.LabelField(rect, "Settings");
            };

            list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
                DrawListItem(rect, index);
            };
        }

        private void DrawListItem(Rect rect, int index) {
            Vector2 ld = GUI.skin.label.CalcSize(new GUIContent("S"));
            SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(index);
            SerializedProperty key = element.FindPropertyRelative("setting");
            SerializedProperty control = element.FindPropertyRelative("control");
            rect.yMin += 3;
            rect.yMax = rect.yMin + ld.y;
            Rect r1 = rect;
            r1.xMax -= rect.width / 2 + 2;
            Rect r2 = rect;
            r2.xMin += rect.width / 2 + 2;
            
            SettingReferenceAttributeDrawer.Draw(r1, key, null);
            EditorGUI.PropertyField(r2, control, GUIContent.none);
        }

        private void ShowList() {
            if (list == null)
                SetupList();

            if (list.index >= list.count)
                list.index = list.count - 1;

            list.DoLayoutList();
        }
    }

}