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
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace SBR.StateSystem {
    [CreateAssetMenu(menuName = "SBR/Component Override")]
    public class ComponentOverride : ScriptableObject {
        [TypeSelect(typeof(Component), true, true, true)]
        [FormerlySerializedAs("typeName")] [SerializeField]
        private string _typeName;
        
        [FormerlySerializedAs("overrides")] [SerializeField]
        private SerializedValueOverride[] _overrides = new SerializedValueOverride[0];
        
        private List<string> _validPaths;
        private Type _type;
        
        public string TypeName {
            get => _typeName;
            set {
                if (_typeName == value) return;
                
                _typeName = value;
                _type = null;
            }
        }
        
        public SerializedValueOverride[] Overrides => _overrides;

        public Type Type {
            get {
                if (_type == null) {
                    _type = Util.GetType(_typeName);
                }
                return _type;
            }
        }

        public List<string> ValidPaths {
            get {
                if (_validPaths == null || _validPaths.Count == 0) {
                    _validPaths = SerializedValueOverride.GetValidPathsForType(Type);
                }

                return _validPaths;
            }
        }

        public IEnumerable<string> UnusedValidPaths => ValidPaths.Where(t => _overrides.All(o => o.Path != t));

        private void OnEnable() {
            Refresh();
        }

        public void NotifyChanged() {
            Refresh();
        }

        public void NotifyChanged(int index) {
            _overrides[index].Initialize(Type);
        }

        public void AddOverride(string path) {
            if (_overrides.Any(t => t.Path == path)) return;
            int pathsIndex = ValidPaths.IndexOf(path);
            if (pathsIndex < 0) return;

            var newObj = new SerializedValueOverride { Path = path };
            newObj.FirstTimeInitialize(Type);

            Array.Resize(ref _overrides, _overrides.Length + 1);
            _overrides[_overrides.Length - 1] = newObj;

            for (int i = _overrides.Length - 1; i > 0; i--) {
                int prevIndex = ValidPaths.IndexOf(_overrides[i - 1].Path);
                if (prevIndex < pathsIndex) {
                    break;
                } else {
                    var temp = _overrides[i];
                    _overrides[i] = _overrides[i - 1];
                    _overrides[i - 1] = temp;
                }
            }
        }

        public void RemoveOverride(int index) {
            for (int i = index; i < _overrides.Length - 1; i++) {
                _overrides[i] = _overrides[i + 1];
            }
            Array.Resize(ref _overrides, _overrides.Length - 1);
        }

        public void CheckValid() {
            if (_overrides == null) return;
            foreach (var value in _overrides) {
                if (!value.Valid) {
                    value.Initialize(Type);
                }
            }
        }

        private void Refresh() {
            _validPaths = null;
            _type = null;

            if (_overrides != null) {
                foreach (var value in _overrides) {
                    value.Initialize(Type);
                }
            }
        }

        public static string DefaultNameForType(Type type) {
            return type.Name + "Override";
        }

        public string displayName {
            get {
                string suffix = DefaultNameForType(Type);
                if (name.EndsWith(suffix)) {
                    return name.Substring(0, name.Length - suffix.Length);
                } else {
                    return name;
                }
            }
        }

        public void ApplyTo(Component component) {
            if (!Type.IsAssignableFrom(component.GetType())) {
                throw new UnityException("Component " + component.name + " of incorrect type.");
            }

            foreach (var item in _overrides) {
                item.SetFieldValue(component);
            }
        }
    }
}