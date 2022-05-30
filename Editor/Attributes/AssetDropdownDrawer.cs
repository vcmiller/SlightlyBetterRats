using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SBR.Editor {
    [CustomPropertyDrawer(typeof(AssetDropdownAttribute))]
    public class AssetDropdownDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            position = EditorGUI.IndentedRect(position);
            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            
            label = EditorGUI.BeginProperty(position, label, property);
            
            Rect contentRect = EditorGUI.PrefixLabel(position, label);

            Rect dropdownRect = contentRect;
            dropdownRect.width = (dropdownRect.width - EditorGUIUtility.standardVerticalSpacing) / 2;
            
            EditorUtil.DoLazyDropdown(dropdownRect, 
                                      new GUIContent(ObjectToString(property.objectReferenceValue)),
                                      () => EditorUtil.GetAssetsOfType(property.GetTypeName()).Prepend(null).ToArray(),
                                      ObjectToString,
                                      t => {
                                          property.serializedObject.Update();
                                          property.objectReferenceValue = t;
                                          property.serializedObject.ApplyModifiedProperties();
                                      });

            Rect normalRect = dropdownRect;
            normalRect.x = dropdownRect.xMax + EditorGUIUtility.standardVerticalSpacing;

            EditorGUI.PropertyField(normalRect, property, GUIContent.none);

            EditorGUI.EndProperty();

            EditorGUI.indentLevel = indent;
        }

        private static string ObjectToString(Object obj) {
            return obj ? obj.name : "<none>";
        }
    }
}