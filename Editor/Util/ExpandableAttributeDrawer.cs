using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DG.DOTweenEditor;
using SBR.Editor;
using UnityEditor;

using UnityEngine;

using Object = UnityEngine.Object;

namespace SBR {
    [CustomPropertyDrawer(typeof(ExpandableAttribute))]
    public class ExpandableAttributeDrawer : PropertyDrawer {
        public static Action<ScriptableObject, string> SaveAction { get; set; } = null;
        private ExpandableAttribute Attribute => (ExpandableAttribute) attribute;
        private const string PPtrText = "PPtr<$";

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            ExpandableAttribute attr = Attribute;
            Object value = property.objectReferenceValue;
            float height = EditorGUI.GetPropertyHeight(property, label);

            if (value != null && (property.isExpanded || attr.AlwaysExpanded)) {
                height += EditorGUIUtility.standardVerticalSpacing + EditorGUIUtility.singleLineHeight;

                SerializedObject so = new SerializedObject(value);
                var iter = so.GetIterator();
                bool first = true;
                while (iter.NextVisible(first)) {
                    if (iter.name == "m_Script") continue;
                    first = false;
                    height += EditorGUIUtility.standardVerticalSpacing +
                              EditorGUI.GetPropertyHeight(iter, new GUIContent(iter.displayName), iter.isExpanded);
                }
            }

            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            ExpandableAttribute attr = Attribute;
            position = EditorGUI.IndentedRect(position);

            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            Rect propertyRect = position;
            propertyRect.height = EditorGUI.GetPropertyHeight(property, label);

            if (SaveAction != null || !string.IsNullOrEmpty(attr.SavePath)) {
                Rect newButtonRect = propertyRect;
                propertyRect.xMax -= 70;
                newButtonRect.xMin = propertyRect.xMax + EditorGUIUtility.standardVerticalSpacing;

                if (GUI.Button(newButtonRect, "New")) {
                    CreateNew(property);
                }
            }

            EditorGUI.PropertyField(propertyRect, property, new GUIContent(property.displayName));

            Object value = property.objectReferenceValue;
            if (value != null && !attr.AlwaysExpanded)
                property.isExpanded = EditorGUI.Foldout(propertyRect, property.isExpanded, GUIContent.none);

            if (value != null && (attr.AlwaysExpanded || property.isExpanded)) {
                EditorGUI.indentLevel = 1;
                Rect propsRect = position;
                propsRect.yMin = propertyRect.yMax + EditorGUIUtility.standardVerticalSpacing;

                SerializedObject so = new SerializedObject(value);

                SerializedProperty nameProp = so.FindProperty("m_Name");
                propsRect.height = EditorGUI.GetPropertyHeight(nameProp, GUIContent.none);
                EditorGUI.PropertyField(propsRect, nameProp);

                propsRect.yMin = propsRect.yMax + EditorGUIUtility.standardVerticalSpacing;

                var iter = so.GetIterator();
                bool first = true;
                while (iter.NextVisible(first)) {
                    if (iter.name == "m_Script") continue;
                    first = false;
                    GUIContent propLabel = new GUIContent(iter.displayName);
                    float propHeight = EditorGUI.GetPropertyHeight(iter, propLabel, iter.isExpanded);
                    propsRect.height = propHeight;
                    EditorGUI.PropertyField(propsRect, iter, propLabel, iter.isExpanded);
                    propsRect.y = propsRect.yMax + EditorGUIUtility.standardVerticalSpacing;
                }

                so.ApplyModifiedProperties();
            }

            EditorGUI.indentLevel = indent;
        }

        private void CreateNew(SerializedProperty property) {
            ExpandableAttribute attr = Attribute;

            if (!property.type.StartsWith(PPtrText)) return;
            string typeName = property.type.Substring(PPtrText.Length, property.type.Length - PPtrText.Length - 1);
            string path = $"New{typeName}.asset";
            if (SaveAction == null) {
                string folder = attr.SavePath;
                string folderWithAssets = Path.Combine("Assets", folder).Replace('\\', '/');
                string fullFolder = Path.Combine(Application.dataPath, folder);
                if (!Directory.Exists(fullFolder)) Directory.CreateDirectory(fullFolder);
                path = EditorUtility.SaveFilePanelInProject("Save New Asset", path,
                                                            "asset", "Save new asset to a location.", folderWithAssets);
                if (string.IsNullOrEmpty(path)) return;
            }

            ScriptableObject asset = ScriptableObject.CreateInstance(typeName);
            if (asset == null) return;
            asset.name = Path.GetFileNameWithoutExtension(path);
            Save(asset, path);

            property.serializedObject.Update();
            property.objectReferenceValue = asset;
            property.serializedObject.ApplyModifiedProperties();
        }

        private static void Save(ScriptableObject asset, string path) {
            if (SaveAction != null) {
                SaveAction(asset, path);
                return;
            }

            string dir = Path.GetDirectoryName(EditorUtil.GetPathRelativeToAssetsFolder(path));
            string fullPath = string.IsNullOrEmpty(dir) ? Application.dataPath : Path.Combine(Application.dataPath, dir);
            if (!Directory.Exists(fullPath)) {
                Directory.CreateDirectory(fullPath);
                AssetDatabase.Refresh();
            }

            string assetPath = path.Replace('\\', '/');
            AssetDatabase.CreateAsset(asset, assetPath);
        }
    }
}