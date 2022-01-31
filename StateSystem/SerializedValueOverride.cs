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
using UnityEngine.Serialization;

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

    public interface IGetSet {
        Type Type { get; }
        string Name { get; }
        MemberInfo Member { get; }
        void SetValue(object obj, object value);
        object GetValue(object obj);
    }

    public class FieldGetSet : IGetSet {
        private readonly FieldInfo _field;
        public Type Type => _field.FieldType;
        public string Name => _field.Name;
        public MemberInfo Member => _field;
        public FieldGetSet(FieldInfo field) => _field = field;
        public object GetValue(object obj) => _field.GetValue(obj);
        public void SetValue(object obj, object value) => _field.SetValue(obj, value);
    }

    public class PropertyGetSet : IGetSet {
        private readonly PropertyInfo _property;
        public Type Type => _property.PropertyType;
        public string Name => _property.Name;
        public MemberInfo Member => _property;
        public PropertyGetSet(PropertyInfo field) => _property = field;
        public object GetValue(object obj) => _property.GetValue(obj);
        public void SetValue(object obj, object value) => _property.SetValue(obj, value);
    }

    [Serializable]
    public class SerializedValueOverride {
        [FormerlySerializedAs("path")] [SerializeField]
        private string _path;

        [FormerlySerializedAs("type")] [SerializeField]
        private SerializableType _type;

        [FormerlySerializedAs("numberValue")] [SerializeField]
        private long _numberValue;

        [FormerlySerializedAs("vectorValue")] [SerializeField]
        private Vector4 _vectorValue;

        [FormerlySerializedAs("stringValue")] [SerializeField]
        private string _stringValue;

        [FormerlySerializedAs("objectReferenceValue")] [SerializeField]
        private Object _objectReferenceValue;

        public string Path {
            get => _path;
            set => _path = value;
        }

        public SerializableType Type => _type;

        public List<IGetSet> FieldChain { get; } = new List<IGetSet>();
        public Type ValueType => FieldChain[FieldChain.Count - 1].Type;
        public bool IsEnumFlags { get; private set; }

        public bool Valid { get; private set; }
        public bool IsProperty => FieldChain[FieldChain.Count - 1] is PropertyGetSet;

        public const int MaxDepth = 7;
        public const BindingFlags BindingFlags = System.Reflection.BindingFlags.Instance |
                                                 System.Reflection.BindingFlags.NonPublic |
                                                 System.Reflection.BindingFlags.Public;

        private static readonly Type[] EmptyTypeArray = new Type[0];
        private static readonly object[] EmptyObjectArray = new object[0];
        private static readonly Dictionary<Type, SerializableType> TypeMap =
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

        public int AncestryLength => FieldChain.Count - 1;
        public string FieldName { get; private set; }

        public object Value {
            get {
                switch (_type) {
                    case SerializableType.Byte:
                        return (byte)_numberValue;
                    case SerializableType.SByte:
                        return (sbyte)_numberValue;
                    case SerializableType.Short:
                        return (short)_numberValue;
                    case SerializableType.UShort:
                        return (ushort)_numberValue;
                    case SerializableType.Int:
                        return (int)_numberValue;
                    case SerializableType.UInt:
                        return (uint)_numberValue;
                    case SerializableType.Long:
                        return _numberValue;
                    case SerializableType.ULong:
                        return (ulong)(_numberValue - long.MinValue);
                    case SerializableType.Float:
                        return _vectorValue.x;
                    case SerializableType.Double:
                        return BitConverter.Int64BitsToDouble(_numberValue);
                    case SerializableType.Bool:
                        return _numberValue != 0;
                    case SerializableType.Char:
                        return (char)_numberValue;
                    case SerializableType.String:
                        return _stringValue;
                    case SerializableType.Enum:
                        return Valid ? Enum.ToObject(ValueType, _numberValue) : null;
                    case SerializableType.Vector2:
                        return (Vector2)_vectorValue;
                    case SerializableType.Vector3:
                        return (Vector3)_vectorValue;
                    case SerializableType.Vector4:
                        return _vectorValue;
                    case SerializableType.Quaternion:
                        return new Quaternion(_vectorValue.x, _vectorValue.y, _vectorValue.z, _vectorValue.w);
                    case SerializableType.Color:
                        return new Color(_vectorValue.x, _vectorValue.y, _vectorValue.z, _vectorValue.w);
                    case SerializableType.LayerMask:
                        return (LayerMask)_numberValue;
                    case SerializableType.ObjectReference:
                        return _objectReferenceValue;
                    default:
                        throw new UnityException("ValueOverride has no type!");
                }
            }

            set {
                _numberValue = 0;
                _stringValue = null;
                _vectorValue = Vector3.zero;
                _objectReferenceValue = null;

                switch (_type) {
                    case SerializableType.Byte:
                    case SerializableType.SByte:
                    case SerializableType.Short:
                    case SerializableType.UShort:
                    case SerializableType.Int:
                    case SerializableType.UInt:
                    case SerializableType.Long:
                    case SerializableType.Char:
                        _numberValue = (long)value;
                        break;
                    case SerializableType.ULong:
                        _numberValue = (long)value + long.MinValue;
                        break;
                    case SerializableType.Float:
                        _vectorValue.x = (float)value;
                        break;
                    case SerializableType.Double:
                        _numberValue = BitConverter.DoubleToInt64Bits((double)value);
                        break;
                    case SerializableType.Bool:
                        _numberValue = ((bool)value) ? 1 : 0;
                        break;
                    case SerializableType.String:
                        _stringValue = (string)value;
                        break;
                    case SerializableType.Enum:
                        _numberValue = Convert.ToInt32((Enum)value);
                        break;
                    case SerializableType.Vector2:
                        _vectorValue = (Vector2)value;
                        break;
                    case SerializableType.Vector3:
                        _vectorValue = (Vector3)value;
                        break;
                    case SerializableType.Vector4:
                        _vectorValue = (Vector4)value;
                        break;
                    case SerializableType.Quaternion:
                        var q = (Quaternion)value;
                        _vectorValue = new Vector4(q.x, q.y, q.z, q.w);
                        break;
                    case SerializableType.Color:
                        Color c = (Color)value;
                        _vectorValue = new Vector4(c.r, c.g, c.b, c.a);
                        break;
                    case SerializableType.LayerMask:
                        _numberValue = (LayerMask)value;
                        break;
                    case SerializableType.ObjectReference:
                        _objectReferenceValue = (Object)value;
                        break;
                    default:
                        throw new UnityException("ValueOverride has no type!");
                }
            }
        }

        public void FirstTimeInitialize(Type baseType) {
            Initialize(baseType);
            _type = GetSerializableType(ValueType);
        }

        public void Initialize(Type baseType) {
            Valid = false;
            var parts = _path.Split('/');
            FieldChain.Clear();

            if (baseType == null) return;

            Type currentType = baseType;
            foreach (string part in parts) {
                FieldInfo field = currentType.GetField(part, BindingFlags);
                PropertyInfo property = currentType.GetProperty(part, BindingFlags);
                if (field != null && IsValidField(field)) {
                    FieldChain.Add(new FieldGetSet(field));
                    currentType = field.FieldType;
                } else if (property != null && IsValidProperty(property)) {
                    FieldChain.Add(new PropertyGetSet(property));
                    currentType = property.PropertyType;
                } else {
                    return;
                }
            }

            IsEnumFlags = (_type == SerializableType.Enum) &&
                Attribute.GetCustomAttribute(ValueType, typeof(FlagsAttribute)) != null;
            FieldName = FieldChain[FieldChain.Count - 1].Name;

            Valid = true;
        }

        public object GetFieldValue(object baseObject) {
            object currentObject = baseObject;
            foreach (IGetSet part in FieldChain) {
                if (currentObject == null) return null;
                currentObject = part.GetValue(currentObject);
            }

            return currentObject;
        }

        public void SetFieldValue(object baseObject) {
            SetFieldValue(baseObject, 0, Value);
        }

        public void SetFieldValue(object baseObject, object finalValue) {
            SetFieldValue(baseObject, 0, finalValue);
        }

        private void SetFieldValue(object currentObject, int index, object finalValue) {
            if (index < FieldChain.Count - 1) {
                object tempValue = FieldChain[index].GetValue(currentObject) ??
                                   FieldChain[index].Type.GetConstructor(EmptyTypeArray).Invoke(EmptyObjectArray);
                SetFieldValue(tempValue, index + 1, finalValue);
                FieldChain[index].SetValue(currentObject, tempValue);
            } else {
                FieldChain[index].SetValue(currentObject, finalValue);
            }
        }

        public static SerializableType GetSerializableType(Type type) {
            if (type == null) {
                return SerializableType.None;
            } else if (TypeMap.ContainsKey(type)) {
                return TypeMap[type];
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

            if (type.IsClass && type.GetConstructor(EmptyTypeArray) == null) {
                return false;
            }

            return true;
        }

        public static List<string> GetValidPathsForType(Type type) {
            var result = new List<string>();
            GetValidPathsForType(type, result, 0, string.Empty);
            return result;
        }

        private static void GetValidPathsForType(Type type, ICollection<string> paths, int depth, string path) {
            SerializableType sType = GetSerializableType(type);
            if (sType != SerializableType.None && !string.IsNullOrEmpty(path)) {
                paths.Add(path);
            } else if (depth < MaxDepth && (depth == 0 || IsSerializableClass(type))) {
                if (!string.IsNullOrEmpty(path)) path += '/';
                foreach (var field in type.GetFields(BindingFlags)) {
                    if (IsValidField(field)) GetValidPathsForType(field.FieldType, paths, depth + 1, path + field.Name);
                }
                foreach (var property in type.GetProperties(BindingFlags)) {
                    if (IsValidProperty(property)) GetValidPathsForType(property.PropertyType, paths, depth + 1, path + property.Name);
                }
            }
        }

        public override string ToString() {
            return $"{_path} ({_type})";
        }
    }
}
