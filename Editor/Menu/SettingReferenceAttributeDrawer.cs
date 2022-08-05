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

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using SBR.Menu;
using System;
using System.Linq;

namespace SBR.Editor {
    [CustomPropertyDrawer(typeof(SettingReferenceAttribute))]
    public class SettingReferenceAttributeDrawer : PropertyDrawer {
        private static Dictionary<Type[], string[]> settingsByType =
            new Dictionary<Type[], string[]>(new TypeArrayEqualityComparer());
        private static string[] allSettings;

        private static string[] GetCachedSettings(Type[] types) {
            if (types == null) {
                if (allSettings == null) {
                    allSettings = SettingsManager.allSettings.Select(s => s.Key).Prepend("None").ToArray();
                }
                return allSettings;
            } else {
                if (!settingsByType.ContainsKey(types)) {
                    settingsByType[types] = types
                        .SelectMany(t => SettingsManager.GetSettings(t))
                        .Select(s => s.Key)
                        .Prepend("None")
                        .ToArray();
                }
                return settingsByType[types];
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            if (property.propertyType != SerializedPropertyType.String) {
                Debug.LogError($"{GetType().Name} can only be used on string property.");
                base.OnGUI(position, property, label);
                return;
            }

            Draw(position, property, label, ((SettingReferenceAttribute)attribute).settingTypes);
        }

        public static void Draw(Rect position, SerializedProperty property, GUIContent label, Type[] types = null) {
            var settings = GetCachedSettings(types);

            int curVal = Array.IndexOf(settings, property.stringValue);
            if (curVal <= 0 || curVal >= settings.Length) {
                curVal = 0;
            }

            if (settings.Length > 0) {
                if (label != null) {
                    curVal = EditorGUI.Popup(position, label.text, curVal, settings);
                } else {
                    curVal = EditorGUI.Popup(position, curVal, settings);
                }

                if (curVal > 0) {
                    property.stringValue = settings[curVal];
                } else {
                    property.stringValue = null;
                }
            } else {
                EditorGUI.LabelField(position, "No settings of given type exist.");
                property.stringValue = null;
            }
        }

        private class TypeArrayEqualityComparer : IEqualityComparer<Type[]> {
            public bool Equals(Type[] x, Type[] y) {
                return x.SequenceEqual(y);
            }

            public int GetHashCode(Type[] obj) {
                const int seed = 487;
                const int modifier = 31;
                
                return obj.Aggregate(seed, (current, item) =>
                    (current * modifier) + item.GetHashCode());
            }
        }
    }
}