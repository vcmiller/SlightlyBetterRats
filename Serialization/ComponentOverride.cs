using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SBR.Serialization {
    [CreateAssetMenu]
    public class ComponentOverride : ScriptableObject {
        [TypeSelect(typeof(Component), true, true, true)]
        public string typeName;
        public SerializedValue[] overrides;

        private Type _type;
        public Type type {
            get {
                if (_type == null) {
                    _type = Util.GetType(typeName);
                }
                return _type;
            }
        }

        [NonSerialized] private List<string> _validPaths;
        public List<string> validPaths {
            get {
                if (_validPaths == null || _validPaths.Count == 0) {
                    _validPaths = SerializedValue.GetValidPathsForType(type);
                }

                return _validPaths;
            }
        }

        public IEnumerable<string> uniqueValidPaths => validPaths.Where(t => !overrides.Any(o => o.path == t));

        private void OnEnable() {
            Refresh();
        }

        public void NotifyChanged() {
            Refresh();
        }

        public void NotifyChanged(int index) {
            overrides[index].Initialize(type);
        }

        public void AddOverride(string path) {
            if (overrides.Any(t => t.path == path)) return;
            int pathsIndex = validPaths.IndexOf(path);
            if (pathsIndex < 0) return;

            var newObj = new SerializedValue();
            newObj.path = path;
            newObj.FirstTimeInitialize(type);

            Array.Resize(ref overrides, overrides.Length + 1);
            overrides[overrides.Length - 1] = newObj;

            for (int i = overrides.Length - 1; i > 0; i--) {
                int prevIndex = validPaths.IndexOf(overrides[i - 1].path);
                if (prevIndex < pathsIndex) {
                    break;
                } else {
                    var temp = overrides[i];
                    overrides[i] = overrides[i - 1];
                    overrides[i - 1] = temp;
                }
            }
        }

        public void RemoveOverride(int index) {
            for (int i = index; i < overrides.Length - 1; i++) {
                overrides[i] = overrides[i + 1];
            }
            Array.Resize(ref overrides, overrides.Length - 1);
        }

        public void CheckValid() {
            foreach (var value in overrides) {
                if (!value.valid) {
                    value.Initialize(type);
                }
            }
        }

        private void Refresh() {
            _validPaths = null;
            _type = null;

            if (overrides != null) {
                foreach (var value in overrides) {
                    value.Initialize(type);
                }
            }
        }

        public void ApplyTo(Component component) {
            if (!type.IsAssignableFrom(component.GetType())) {
                throw new UnityException("Component " + component.name + " of incorrect type.");
            }

            foreach (var item in overrides) {
                item.SetFieldValue(component);
            }
        }
    }
}