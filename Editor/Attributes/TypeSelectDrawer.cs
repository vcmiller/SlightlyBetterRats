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

using UnityEngine;
using UnityEditor;
using System.Linq;
using System;
using UnityEditor.IMGUI.Controls;

namespace SBR.Editor {
    [CustomPropertyDrawer(typeof(TypeSelectAttribute))]
    public class TypeSelectDrawer : PropertyDrawer {
        private string[] types;
        private SearchField searchField;
        private string search = "";

        private void OnSearch() {

        }

        // Draw the property inside the given rect
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

            // First get the attribute since it contains the range for the slider
            TypeSelectAttribute attr = (TypeSelectAttribute) attribute;

            // Now draw the property as a Slider or an IntSlider based on whether it's a float or integer.
            if (property.propertyType == SerializedPropertyType.String) {
                if (searchField == null && attr.search) {
                    searchField = new SearchField();
                }

                if (types == null) {
                    var tempTypes = Util.AllTypes.Where(p => !p.IsInterface &&
                                                             (attr.allowGeneric || !p.IsGenericType) &&
                                                             (attr.allowAbstract || !p.IsAbstract) &&
                                                             attr.baseClass.IsAssignableFrom(p));

                    if (attr.search) {
                        tempTypes = tempTypes.Where(t => t.Name.ToLower().Contains(search));
                    }

                    var cur = Util.GetType(property.stringValue);
                    if (cur != null && !tempTypes.Contains(cur)) tempTypes.Append(cur);

                    types = tempTypes.Select(t => t.FullName).Prepend("(none)").ToArray();
                }

                int index = Array.IndexOf(types, property.stringValue);
                if (index < 0) {
                    index = Mathf.Max(Array.IndexOf(types, attr.baseClass.FullName), 0);
                }

                EditorGUI.BeginProperty(position, label, property);
                if (attr.search) {
                    EditorGUI.LabelField(new Rect(position.x, position.y, position.width / 3, position.height), label);
                    var newSearch = searchField.OnGUI(new Rect(position.x + position.width / 3, position.y, position.width / 3 - 5, position.height), search);
                    index = EditorGUI.Popup(new Rect(position.x + position.width * 2 / 3, position.y, position.width / 3, position.height), index, types);
                    property.stringValue = types[index];

                    if (newSearch != search) {
                        search = newSearch.ToLower();
                        types = null;
                    }
                } else {
                    EditorGUI.LabelField(new Rect(position.x, position.y, position.width / 2, position.height), label);
                    index = EditorGUI.Popup(new Rect(position.x + position.width / 2, position.y, position.width / 2, position.height), index, types);
                    property.stringValue = types[index];
                }
                EditorGUI.EndProperty();
            } else {
                EditorGUI.LabelField(position, label.text, "Use TypeSelect with string.");
            }
        }
    }
}