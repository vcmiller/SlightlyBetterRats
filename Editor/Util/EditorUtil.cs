﻿// MIT License
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
using System.Reflection;
using System;

namespace SBR.Editor {
    public static class EditorUtil {
        private static BuildTargetGroup[] buildTargetGroups = new BuildTargetGroup[] {
            BuildTargetGroup.Standalone,
            BuildTargetGroup.Android,
            BuildTargetGroup.iOS,
            BuildTargetGroup.WSA
        };

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
                    int len = "data[".Length;
                    int index = int.Parse(path.Substring(len, path.LastIndexOf("]", StringComparison.Ordinal) - len));
                    if (reflectionTarget is Array array) {
                        reflectionTarget = array.GetValue(index);
                    } else if (reflectionTarget is IList list) {
                        reflectionTarget = list[index];
                    }
                } else {
                    FieldInfo fieldInfo = null;
                    var tempTarget = reflectionTarget.GetType();
                    while (fieldInfo == null && tempTarget != null) {
                        fieldInfo = tempTarget.GetField(path, BindingFlags.NonPublic |
                                                                        BindingFlags.Instance |
                                                                        BindingFlags.Public);
                        tempTarget = tempTarget.BaseType;
                    }
                    reflectionTarget = fieldInfo.GetValue(reflectionTarget);
                }
            }
            return (T)reflectionTarget;
        }

        public static List<string> GetDefinesList(BuildTargetGroup group) {
            return new List<string>(PlayerSettings.GetScriptingDefineSymbolsForGroup(group).Split(';'));
        }

        public static void SetSymbolDefined(string symbol, bool value, BuildTargetGroup group) {
            var defines = GetDefinesList(group);
            if (value) {
                if (defines.Contains(symbol)) {
                    return;
                }
                defines.Add(symbol);
            } else {
                if (!defines.Contains(symbol)) {
                    return;
                }
                while (defines.Contains(symbol)) {
                    defines.Remove(symbol);
                }
            }
            string definesString = string.Join(";", defines.ToArray());
            PlayerSettings.SetScriptingDefineSymbolsForGroup(group, definesString);
        }

        public static void SetSymbolDefined(string symbol, bool value) {
            foreach (var group in buildTargetGroups) {
                SetSymbolDefined(symbol, value, group);
            }
        }
    }
}