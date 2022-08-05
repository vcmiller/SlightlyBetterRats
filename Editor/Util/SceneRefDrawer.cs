// The MIT License (MIT)
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

using Infohazard.Core;
using UnityEditor;

using UnityEngine;

namespace SBR.Editor {
    [CustomPropertyDrawer(typeof(SceneRef))]
    public class SceneRefDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            label = EditorGUI.BeginProperty(position, label, property);
            
            SerializedProperty nameProp = property.FindPropertyRelative("_path");
            SerializedProperty guidProp = property.FindPropertyRelative("_guid");
            SceneAsset scene = string.IsNullOrEmpty(nameProp.stringValue)
                ? null
                : AssetDatabase.LoadAssetAtPath<SceneAsset>(nameProp.stringValue);
            
            if (!scene && !string.IsNullOrEmpty(guidProp.stringValue)) {
                foreach (EditorBuildSettingsScene buildScene in EditorBuildSettings.scenes) {
                    if (buildScene.guid.ToString() == guidProp.stringValue) {
                        scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(buildScene.path);
                        break;
                    }
                }
                
                if (scene) {
                    nameProp.stringValue = AssetDatabase.GetAssetPath(scene);
                }
            }
            
            EditorGUI.BeginChangeCheck();
            var newScene = (SceneAsset)EditorGUI.ObjectField(position, label, scene, typeof(SceneAsset), false);

            if (EditorGUI.EndChangeCheck())
            {
                var newPath = AssetDatabase.GetAssetPath(newScene);
                nameProp.stringValue = newPath;
            }

            if (newScene) {
                guidProp.stringValue = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(newScene));
            }
            
            EditorGUI.EndProperty();
        }
    }
}
