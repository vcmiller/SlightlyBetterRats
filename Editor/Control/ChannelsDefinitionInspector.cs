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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System;
using UnityEditorInternal;

namespace SBR.Editor {
    [CustomEditor(typeof(ChannelsDefinition))]
    public class ChannelsDefinitionInspector : UnityEditor.Editor {
        private ReorderableList channelList;

        private void OnEnable() {
            channelList = new ReorderableList(serializedObject, serializedObject.FindProperty("channels"), true, true, true, true);
            channelList.drawElementCallback = DrawListElement;
            channelList.elementHeight *= 4;
            channelList.drawHeaderCallback = (Rect rect) => {
                EditorGUI.LabelField(rect, "Channel List");
            };
        }

        private void DrawListElement(Rect rect, int index, bool isActive, bool isFocused) {
            var element = channelList.serializedProperty.GetArrayElementAtIndex(index);

            rect.yMin += 4;
            EditorGUI.PropertyField(rect, element);
        }

        public override void OnInspectorGUI() {
            if (GUILayout.Button("Generate Class")) {
                var path = AssetDatabase.GetAssetPath(target);
                if (path.Length > 0) {
                    ChannelsClassGenerator.GenerateClass(target as ChannelsDefinition);
                }
            }

            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, "channels", "m_Script");
            channelList.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }
    }
}