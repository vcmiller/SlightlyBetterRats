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