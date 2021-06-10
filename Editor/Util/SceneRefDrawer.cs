using System.Collections;
using System.Collections.Generic;

using UnityEditor;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace SBR.Editor {
    [CustomPropertyDrawer(typeof(SceneRef))]
    public class SceneRefDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            label = EditorGUI.BeginProperty(position, label, property);
            
            SerializedProperty nameProp = property.FindPropertyRelative("_path");
            SceneAsset scene = string.IsNullOrEmpty(nameProp.stringValue)
                ? null
                : AssetDatabase.LoadAssetAtPath<SceneAsset>(nameProp.stringValue);
            
            EditorGUI.BeginChangeCheck();
            var newScene = (SceneAsset)EditorGUI.ObjectField(position, label, scene, typeof(SceneAsset), false);

            if (EditorGUI.EndChangeCheck())
            {
                var newPath = AssetDatabase.GetAssetPath(newScene);
                nameProp.stringValue = newPath;
            }
            
            EditorGUI.EndProperty();
        }
    }
}
