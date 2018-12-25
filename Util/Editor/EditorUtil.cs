using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System;

public static class EditorUtil {

    /// <summary>
    /// Get the value of a SerializedProperty of any type.
    /// Does not work for serializable objects or gradients.
    /// Enum values are returned as ints.
    /// </summary>
    /// <param name="property">The property to read.</param>
    /// <returns>The value of the property, or null if not readable.</returns>
    public static object GetValue(this SerializedProperty property) {
        switch (property.propertyType) {
            case SerializedPropertyType.Generic:
                return null;
            case SerializedPropertyType.Integer:
                return property.intValue;
            case SerializedPropertyType.Boolean:
                return property.boolValue;
            case SerializedPropertyType.Float:
                return property.floatValue;
            case SerializedPropertyType.String:
                return property.stringValue;
            case SerializedPropertyType.Color:
                return property.colorValue;
            case SerializedPropertyType.ObjectReference:
                return property.objectReferenceValue;
            case SerializedPropertyType.LayerMask:
                return (LayerMask)property.intValue;
            case SerializedPropertyType.Enum:
                return property.enumValueIndex;
            case SerializedPropertyType.Vector2:
                return property.vector2Value;
            case SerializedPropertyType.Vector3:
                return property.vector3Value;
            case SerializedPropertyType.Vector4:
                return property.vector4Value;
            case SerializedPropertyType.Rect:
                return property.rectValue;
            case SerializedPropertyType.ArraySize:
                return property.arraySize;
            case SerializedPropertyType.Character:
                return (char)property.intValue;
            case SerializedPropertyType.AnimationCurve:
                return property.animationCurveValue;
            case SerializedPropertyType.Bounds:
                return property.boundsValue;
            case SerializedPropertyType.Gradient:
                return null;
            case SerializedPropertyType.Quaternion:
                return property.quaternionValue;
            case SerializedPropertyType.ExposedReference:
                return property.exposedReferenceValue;
            case SerializedPropertyType.FixedBufferSize:
                return property.fixedBufferSize;
            case SerializedPropertyType.Vector2Int:
                return property.vector2IntValue;
            case SerializedPropertyType.Vector3Int:
                return property.vector3IntValue;
            case SerializedPropertyType.RectInt:
                return property.rectIntValue;
            case SerializedPropertyType.BoundsInt:
                return property.boundsIntValue;
            default:
                return null;
        }
    }

    /// <summary>
    /// Try to find the value of a given property using reflection.
    /// </summary>
    /// <param name="prop">The property to read.</param>
    /// <returns>The value of the property.</returns>
    public static object FindValue(this SerializedProperty prop) {
        return prop.FindValue<object>();
    }

    // From Unify Community Wiki
    /// <summary>
    /// Find the value of a given property using Reflection.
    /// </summary>
    /// <typeparam name="T">The type to cast the value to.</typeparam>
    /// <param name="prop">The property to read</param>
    /// <returns>The value of the property.</returns>
    public static T FindValue<T>(this SerializedProperty prop) {
        string[] separatedPaths = prop.propertyPath.Split('.');

        object reflectionTarget = prop.serializedObject.targetObject as object;


        for (int i = 0; i < separatedPaths.Length; i++) {
            string path = separatedPaths[i];
            if (path == "Array" && i < separatedPaths.Length - 1 && separatedPaths[i + 1].StartsWith("data[")) {
                continue;
            } else if (path.StartsWith("data[")) {
                Array array = (Array)reflectionTarget;
                int len = "data[".Length;
                int index = int.Parse(path.Substring(len, path.LastIndexOf("]") - len));
                reflectionTarget = array.GetValue(index);
            } else {
                FieldInfo fieldInfo = reflectionTarget.GetType().GetField(path);
                reflectionTarget = fieldInfo.GetValue(reflectionTarget);
            }
        }
        return (T)reflectionTarget;
    }
}
