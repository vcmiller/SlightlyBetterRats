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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SBR.StateSystem {
    public enum SerializableType {
        None,
        Byte,
        SByte,
        Short,
        UShort,
        Int,
        UInt,
        Long,
        ULong,
        Float,
        Double,
        Bool,
        Char,
        String,
        Enum,
        Vector2,
        Vector3,
        Vector4,
        Quaternion,
        Color,
        LayerMask,
        ObjectReference
    }

    public interface GetSet {
        void SetValue(object obj, object value);
        object GetValue(object obj);
        Type type { get; }
        string name { get; }
        MemberInfo member { get; }
    }

    public class FieldGetSet : GetSet {
        private FieldInfo field;
        public FieldGetSet(FieldInfo field) => this.field = field;
        public object GetValue(object obj) => field.GetValue(obj);
        public void SetValue(object obj, object value) => field.SetValue(obj, value);
        public Type type => field.FieldType;
        public string name => field.Name;
        public MemberInfo member => field;
    }

    public partial class PropertyGetSet : GetSet {
        private PropertyInfo property;
        public PropertyGetSet(PropertyInfo field) => this.property = field;
        public object GetValue(object obj) => property.GetValue(obj);
        public void SetValue(object obj, object value) => property.SetValue(obj, value);
        public Type type => property.PropertyType;
        public string name => property.Name;
        public MemberInfo member => property;
    }

    [Serializable]
    public class SerializedValue {
        public string path;
        public SerializableType type;

        [SerializeField] private long numberValue;
        [SerializeField] private Vector4 vectorValue;
        [SerializeField] private string stringValue;
        [SerializeField] private Object objectReferenceValue;

        public List<GetSet> fieldChain { get; private set; } = new List<GetSet>();
        public Type valueType => fieldChain[fieldChain.Count - 1].type;
        public bool isEnumFlags { get; private set; }

        public bool valid { get; private set; }
        public bool isProperty => fieldChain[fieldChain.Count - 1] is PropertyGetSet;

        public const int MaxDepth = 7;
        public const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

        private static readonly Type[] emptyTypeArray = new Type[0];
        private static readonly object[] emptyObjectArray = new object[0];
        private static readonly Dictionary<Type, SerializableType> typeMap =
            new Dictionary<Type, SerializableType>() {
                { typeof(byte), SerializableType.Byte },
                { typeof(sbyte), SerializableType.SByte },
                { typeof(short), SerializableType.Short },
                { typeof(ushort), SerializableType.UShort },
                { typeof(int), SerializableType.Int },
                { typeof(uint), SerializableType.UInt },
                { typeof(long), SerializableType.Long },
                { typeof(ulong), SerializableType.ULong },
                { typeof(float), SerializableType.Float },
                { typeof(double), SerializableType.Double },
                { typeof(bool), SerializableType.Bool },
                { typeof(char), SerializableType.Char },
                { typeof(string), SerializableType.String },
                // Enum handled below
                { typeof(Vector2), SerializableType.Vector2 },
                { typeof(Vector3), SerializableType.Vector3 },
                { typeof(Vector4), SerializableType.Vector4 },
                { typeof(Quaternion), SerializableType.Quaternion },
                { typeof(Color), SerializableType.Color },
                { typeof(LayerMask), SerializableType.LayerMask }
                // Object reference handled below
            };

        public int ancestryLength => fieldChain.Count - 1;
        public string fieldName { get; private set; }
        public string displayName { get; private set; }

        public object value {
            get {
                switch (type) {
                    case SerializableType.Byte:
                        return (byte)numberValue;
                    case SerializableType.SByte:
                        return (sbyte)numberValue;
                    case SerializableType.Short:
                        return (short)numberValue;
                    case SerializableType.UShort:
                        return (ushort)numberValue;
                    case SerializableType.Int:
                        return (int)numberValue;
                    case SerializableType.UInt:
                        return (uint)numberValue;
                    case SerializableType.Long:
                        return numberValue;
                    case SerializableType.ULong:
                        return (ulong)(numberValue - long.MinValue);
                    case SerializableType.Float:
                        return vectorValue.x;
                    case SerializableType.Double:
                        return BitConverter.Int64BitsToDouble(numberValue);
                    case SerializableType.Bool:
                        return numberValue != 0;
                    case SerializableType.Char:
                        return (char)numberValue;
                    case SerializableType.String:
                        return stringValue;
                    case SerializableType.Enum:
                        return valid ? Enum.ToObject(valueType, numberValue) : null;
                    case SerializableType.Vector2:
                        return (Vector2)vectorValue;
                    case SerializableType.Vector3:
                        return (Vector3)vectorValue;
                    case SerializableType.Vector4:
                        return vectorValue;
                    case SerializableType.Quaternion:
                        return new Quaternion(vectorValue.x, vectorValue.y, vectorValue.z, vectorValue.w);
                    case SerializableType.Color:
                        return new Color(vectorValue.x, vectorValue.y, vectorValue.z, vectorValue.w);
                    case SerializableType.LayerMask:
                        return (LayerMask)numberValue;
                    case SerializableType.ObjectReference:
                        return objectReferenceValue;
                    default:
                        throw new UnityException("ValueOverride has no type!");
                }
            }

            set {
                numberValue = 0;
                stringValue = null;
                vectorValue = Vector3.zero;
                objectReferenceValue = null;

                switch (type) {
                    case SerializableType.Byte:
                    case SerializableType.SByte:
                    case SerializableType.Short:
                    case SerializableType.UShort:
                    case SerializableType.Int:
                    case SerializableType.UInt:
                    case SerializableType.Long:
                    case SerializableType.Char:
                        numberValue = (long)value;
                        break;
                    case SerializableType.ULong:
                        numberValue = (long)value + long.MinValue;
                        break;
                    case SerializableType.Float:
                        vectorValue.x = (float)value;
                        break;
                    case SerializableType.Double:
                        numberValue = BitConverter.DoubleToInt64Bits((double)value);
                        break;
                    case SerializableType.Bool:
                        numberValue = ((bool)value) ? 1 : 0;
                        break;
                    case SerializableType.String:
                        stringValue = (string)value;
                        break;
                    case SerializableType.Enum:
                        numberValue = Convert.ToInt32((Enum)value);
                        break;
                    case SerializableType.Vector2:
                        vectorValue = (Vector2)value;
                        break;
                    case SerializableType.Vector3:
                        vectorValue = (Vector3)value;
                        break;
                    case SerializableType.Vector4:
                        vectorValue = (Vector4)value;
                        break;
                    case SerializableType.Quaternion:
                        var q = (Quaternion)value;
                        vectorValue = new Vector4(q.x, q.y, q.z, q.w);
                        break;
                    case SerializableType.Color:
                        Color c = (Color)value;
                        vectorValue = new Vector4(c.r, c.g, c.b, c.a);
                        break;
                    case SerializableType.LayerMask:
                        numberValue = (LayerMask)value;
                        break;
                    case SerializableType.ObjectReference:
                        objectReferenceValue = (Object)value;
                        break;
                    default:
                        throw new UnityException("ValueOverride has no type!");
                }
            }
        }

        public void FirstTimeInitialize(Type baseType) {
            Initialize(baseType);
            type = GetSerializableType(valueType);
        }

        public void Initialize(Type baseType) {
            valid = false;
            var parts = path.Split('/');
            fieldChain.Clear();

            if (baseType == null) return;

            Type currentType = baseType;
            foreach (var part in parts) {
                var field = currentType.GetField(part, bindingFlags);
                var property = currentType.GetProperty(part, bindingFlags);
                if (field != null && IsValidField(field)) {
                    fieldChain.Add(new FieldGetSet(field));
                    currentType = field.FieldType;
                } else if (property != null && IsValidProperty(property)) {
                    fieldChain.Add(new PropertyGetSet(property));
                    currentType = property.PropertyType;
                } else {
                    return;
                }
            }

            isEnumFlags = (type == SerializableType.Enum) &&
                Attribute.GetCustomAttribute(fieldChain[fieldChain.Count - 1].member, typeof(MultiEnumAttribute)) != null;
            fieldName = fieldChain[fieldChain.Count - 1].name;
            displayName = Util.SplitCamelCase(fieldName, true);

            valid = true;
        }

        public object GetFieldValue(object baseObject) {
            object currentObject = baseObject;
            foreach (var part in fieldChain) {
                if (currentObject == null) return null;
                currentObject = part.GetValue(currentObject);
            }

            return currentObject;
        }

        public void SetFieldValue(object baseObject) {
            SetFieldValue(baseObject, 0, value);
        }

        public void SetFieldValue(object baseObject, object finalValue) {
            SetFieldValue(baseObject, 0, finalValue);
        }

        private void SetFieldValue(object currentObject, int index, object finalValue) {
            if (index < fieldChain.Count - 1) {
                object tempValue = fieldChain[index].GetValue(currentObject);
                if (tempValue == null) {
                    tempValue = fieldChain[index].type.GetConstructor(emptyTypeArray).Invoke(emptyObjectArray);
                }
                SetFieldValue(tempValue, index + 1, finalValue);
                fieldChain[index].SetValue(currentObject, tempValue);
            } else {
                fieldChain[index].SetValue(currentObject, finalValue);
            }
        }

        public static SerializableType GetSerializableType(Type type) {
            if (type == null) {
                return SerializableType.None;
            } else if (typeMap.ContainsKey(type)) {
                return typeMap[type];
            } else if (type.IsEnum) {
                return SerializableType.Enum;
            } else if (typeof(Object).IsAssignableFrom(type)) {
                return SerializableType.ObjectReference;
            } else {
                return SerializableType.None;
            }
        }

        private static bool IsValidField(FieldInfo field) {
            if (field.IsInitOnly) return false;
            if (!field.IsPublic && field.GetCustomAttribute(typeof(SerializeField)) == null) return false;
            if (field.GetCustomAttribute(typeof(NonSerializedAttribute)) != null) return false;
            if (field.GetCustomAttribute(typeof(NoOverridesAttribute)) != null) return false;
            return true;
        }

        private static bool IsValidProperty(PropertyInfo property) {
            if (property.GetGetMethod() == null || property.GetSetMethod() == null) return false;
            if (!property.GetGetMethod().IsPublic || !property.GetSetMethod().IsPublic) return false;
            if (property.GetCustomAttribute(typeof(NonSerializedAttribute)) != null) return false;
            if (property.GetCustomAttribute(typeof(NoOverridesAttribute)) != null) return false;
            return true;
        }

        public static bool IsSerializableClass(Type type) {
            if (!(type.IsClass || (type.IsValueType && !type.IsPrimitive && !type.IsEnum))) {
                return false;
            }

            if (type.IsAbstract || type.IsInterface) {
                return false;
            }

            if (type.IsClass && type.GetConstructor(emptyTypeArray) == null) {
                return false;
            }

            return true;
        }

        public static List<string> GetValidPathsForType(Type type) {
            var result = new List<string>();
            GetValidPathsForType(type, result, 0, string.Empty);
            return result;
        }

        private static void GetValidPathsForType(Type type, List<string> paths, int depth, string path) {
            var sType = GetSerializableType(type);
            if (sType != SerializableType.None && !string.IsNullOrEmpty(path)) {
                paths.Add(path);
            } else if (depth < MaxDepth && (depth == 0 || IsSerializableClass(type))) {
                if (!string.IsNullOrEmpty(path)) path += '/';
                foreach (var field in type.GetFields(bindingFlags)) {
                    if (IsValidField(field)) GetValidPathsForType(field.FieldType, paths, depth + 1, path + field.Name);
                }
                foreach (var property in type.GetProperties(bindingFlags)) {
                    if (IsValidProperty(property)) GetValidPathsForType(property.PropertyType, paths, depth + 1, path + property.Name);
                }
            }
        }

        public override string ToString() {
            return $"{path} ({type})";
        }
    }
}
