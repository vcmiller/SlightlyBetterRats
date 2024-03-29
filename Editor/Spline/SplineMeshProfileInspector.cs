﻿// The MIT License (MIT)
// 
// Copyright (c) 2022-present Vincent Miller
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

using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace SBR.Editor {
    [CustomEditor(typeof(SplineMeshProfile))]
    public class SplineMeshProfileInspector : UnityEditor.Editor {
        private ReorderableList meshList;
        private SplineMeshProfile profile { get { return target as SplineMeshProfile; } }

        private void OnEnable() {
            meshList = new ReorderableList(serializedObject, serializedObject.FindProperty("meshes"), true, true, true, true);
            meshList.drawElementCallback = DrawListElement;
            meshList.elementHeight *= 4;
            meshList.drawHeaderCallback = (Rect rect) => {
                EditorGUI.LabelField(rect, "Mesh Sequence");
            };
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, "meshes", "m_Script");
            meshList.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawListElement(Rect rect, int index, bool isActive, bool isFocused) {
            var element = meshList.serializedProperty.GetArrayElementAtIndex(index);
            float l = EditorGUIUtility.singleLineHeight;
            
            rect.y += 2;
            float labelSpacing = 10;
            float colSpacing = 10;

            float labelWidth = labelSpacing + 65;

            float colWidth = (rect.width - labelWidth * 2 - colSpacing) / 2;
            EditorGUI.LabelField(new Rect(rect.x, rect.y, labelWidth, l), 
                new GUIContent("Render", "Mesh used for rendering."));
            EditorGUI.PropertyField(
                new Rect(rect.x + labelWidth, rect.y, colWidth, l),
                element.FindPropertyRelative("render"), GUIContent.none);

            EditorGUI.LabelField(new Rect(rect.x + labelWidth + colWidth + colSpacing, rect.y, labelWidth, l), 
                new GUIContent("Collision", "Mesh used for collision. Only used if separateCollisionMesh is true."));
            EditorGUI.PropertyField(
                new Rect(rect.x + labelWidth * 2 + colWidth + colSpacing, rect.y, colWidth, l),
                element.FindPropertyRelative("collision"), GUIContent.none);
            
            EditorGUI.LabelField(new Rect(rect.x, rect.y + l + 4, labelWidth, l), 
                new GUIContent("Stretch", "How (if at all) the mesh is allowed to stretch in order to completely fill the spline."));
            EditorGUI.PropertyField(
                new Rect(rect.x + labelWidth, rect.y + l + 4, colWidth, l),
                element.FindPropertyRelative("stretchMode"), GUIContent.none);
            
            EditorGUI.LabelField(new Rect(rect.x + labelWidth + colWidth + colSpacing, rect.y + l + 4, labelWidth, l), 
                new GUIContent("Repeats", "How many times to repeat the mesh each cycle. Values <= 0 are ignored."));
            EditorGUI.PropertyField(
                new Rect(rect.x + labelWidth * 2 + colWidth + colSpacing, rect.y + l + 4, colWidth, l),
                element.FindPropertyRelative("repeat"), GUIContent.none);

            EditorGUI.LabelField(new Rect(rect.x, rect.y + (l + 4) * 2, labelWidth, l), 
                new GUIContent("Gap Before", "The amount of empty space before each repetition of the mesh."));
            EditorGUI.PropertyField(
                new Rect(rect.x + labelWidth, rect.y + (l + 4) * 2, colWidth, l),
                element.FindPropertyRelative("gapBefore"), GUIContent.none);

            EditorGUI.LabelField(new Rect(rect.x + labelWidth + colWidth + colSpacing, rect.y + (l + 4) * 2, labelWidth, l), 
                new GUIContent("Gap After", "The amount of empty space after each repetition of the mesh."));
            EditorGUI.PropertyField(
                new Rect(rect.x + labelWidth * 2 + colWidth + colSpacing, rect.y + (l + 4) * 2, colWidth, l),
                element.FindPropertyRelative("gapAfter"), GUIContent.none);

            EditorGUI.LabelField(new Rect(rect.x, rect.y + (l + 4) * 3, labelWidth, l), 
                new GUIContent("Alignment", "How the mesh will be aligned on the spline."));
            EditorGUI.PropertyField(
                new Rect(rect.x + labelWidth, rect.y + (l + 4) * 3, colWidth, l),
                element.FindPropertyRelative("alignMode"), GUIContent.none);
        }
    }

}