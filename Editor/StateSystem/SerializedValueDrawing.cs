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

using SBR.StateSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SBR.Editor {
    public static class SerializedValueDrawing {
        private static bool _loaded;
        private static Texture2D _iconProperty;
        private static Texture2D _iconField;
        private static Texture2D _iconMissing;

        private static void InitResources() {
            if (_loaded) return;

            _iconProperty = Resources.Load<Texture2D>("IconProperty");
            _iconField = Resources.Load<Texture2D>("IconField");
            _iconMissing = Resources.Load<Texture2D>("IconMissing");
            _loaded = true;
        }

        public static void UpdateHeading(SerializedValueOverride property, List<string> current) {
            string[] propPath = property.Path.Split('/');
            while (true) {
                string join = string.Join("/", current);
                if (current.Count > 0) {
                    join += "/";
                }
                if (property.Path.StartsWith(join)) {
                    break;
                }

                current.RemoveAt(current.Count - 1);
                EditorGUI.indentLevel--;
            }

            while (current.Count < propPath.Length - 1) {
                IGetSet field = property.FieldChain.Count > current.Count ? property.FieldChain[current.Count] : null;
                string name = propPath[current.Count];
                string newName = name;
                current.Add(newName);

                var label = new GUIContent(ObjectNames.NicifyVariableName(newName),
                                           field != null
                                               ? field is PropertyGetSet ? _iconProperty : _iconField
                                               : _iconMissing);
                EditorGUI.BeginDisabledGroup(field == null);
                EditorGUILayout.LabelField(label);
                EditorGUI.EndDisabledGroup();
                EditorGUI.indentLevel++;
            }
        }

        public static void DrawLayout(Object reference, SerializedValueOverride property) {
            InitResources();
            GUIContent label = new GUIContent(ObjectNames.NicifyVariableName(property.FieldName));
            if (property.Valid) {
                label.image = property.IsProperty ? _iconProperty : _iconField;
            } else {
                label.image = _iconMissing;
            }

            EditorGUI.BeginDisabledGroup(!property.Valid);
            object value = property.Value;
            object newValue = value;
            switch (property.Type) {
                case SerializableType.Byte:
                case SerializableType.SByte:
                case SerializableType.Short:
                case SerializableType.UShort:
                case SerializableType.Int:
                case SerializableType.UInt:
                    newValue = (long)EditorGUILayout.IntField(label, (int)value);
                    break;
                case SerializableType.Long:
                case SerializableType.ULong:
                    newValue = EditorGUILayout.LongField(label, (long)value);
                    break;
                case SerializableType.Float:
                    newValue = EditorGUILayout.FloatField(label, (float)value);
                    break;
                case SerializableType.Double:
                    newValue = EditorGUILayout.DoubleField(label, (double)value);
                    break;
                case SerializableType.Bool:
                    newValue = EditorGUILayout.Toggle(label, (bool)value);
                    break;
                case SerializableType.Char:
                    string result = EditorGUILayout.TextField(label, ((char)value).ToString());
                    if (result.Length > 0) {
                        newValue = (long)result[0];
                    } else {
                        newValue = (long)'\0';
                    }
                    break;
                case SerializableType.String:
                    newValue = EditorGUILayout.TextField(label, (string)value);
                    break;
                case SerializableType.Enum:
                    if (value == null) {
                        EditorGUILayout.LabelField(label, new GUIContent("Invalid Enum"));
                    } else if (property.IsEnumFlags) {
                        newValue = EditorGUILayout.EnumFlagsField(label, (Enum)value);
                    } else {
                        newValue = EditorGUILayout.EnumPopup(label, (Enum)value);
                    }
                    break;
                case SerializableType.Vector2:
                    newValue = EditorGUILayout.Vector2Field(label, (Vector2)value);
                    break;
                case SerializableType.Vector3:
                    newValue = EditorGUILayout.Vector3Field(label, (Vector3)value);
                    break;
                case SerializableType.Vector4:
                    newValue = EditorGUILayout.Vector4Field(label, (Vector4)value);
                    break;
                case SerializableType.Quaternion:
                    var q = (Quaternion)value;
                    Vector4 v = EditorGUILayout.Vector4Field(label, new Vector4(q.x, q.y, q.z, q.w));
                    newValue = new Quaternion(v.x, v.y, v.z, v.w);
                    break;
                case SerializableType.Color:
                    newValue = EditorGUILayout.ColorField(label, (Color)value);
                    break;
                case SerializableType.LayerMask:
                    LayerMask tempMask = EditorGUILayout.MaskField(label,
                        InternalEditorUtility.LayerMaskToConcatenatedLayersMask((LayerMask)value),
                        InternalEditorUtility.layers);
                    newValue = InternalEditorUtility.ConcatenatedLayersMaskToLayerMask(tempMask);
                    break;
                case SerializableType.ObjectReference:
                    newValue = EditorGUILayout.ObjectField(label, (Object)value, property.Valid ? property.ValueType : typeof(Object), false);
                    break;
            }
            if (!Equals(value, newValue)) {
                Undo.RecordObject(reference, "Change Override Value");
                property.Value = newValue;
                EditorUtility.SetDirty(reference);
            }
            EditorGUI.EndDisabledGroup();
        }
    }
}