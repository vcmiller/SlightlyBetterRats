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

using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace SBR.Editor {
    [InitializeOnLoad]
    public static class TagsGenerator {
        public const string checkTagsPref = "CheckTags";
        private const string template = @"using System;

namespace SBR {{
#if TagsGenerated
    [Flags]
    public enum Tag {{
        {0}
    }}
#endif
}}
";
        private static bool didGenerate = false;
        
        private static string[] _tagsValues;
        private static string[] tagsValues {
            get {
                if (_tagsValues == null) {
                    _tagsValues = Enum.GetNames(typeof(Tag));
                }
                return _tagsValues;
            }
        }

        static TagsGenerator() {
            EditorApplication.update += CheckTags;
        }

        private static void CheckTags() {
            if (didGenerate || !EditorPrefs.GetBool(checkTagsPref, true)) {
                return;
            }
            var tags = InternalEditorUtility.tags;

            bool needsRegen = !Enumerable.SequenceEqual(tags.Where(ValidEnumValue), tagsValues);

            if (needsRegen) {
                if (EditorUtility.DisplayDialog("Generate Tags", "Do you want to generate a Tag.cs file?", "OK", "No")) {
                    DoGenerate();
                } else {
                    EditorPrefs.SetBool(checkTagsPref, false);
                }
            }
        }

        private static string GetEnumValues() {
            var tags = InternalEditorUtility.tags;
            var str = new StringBuilder();
            int max = Mathf.Min(tags.Length, 32);
            int current = 0;
            for (int i = 0; i < max; i++) {
                // Need to skip any tags with spaces, as they'll cause a compiler error.
                // We could just remove the spaces, but the conversion to an actual Unity tag wouldn't work.
                // So better to just drop them from the enum.
                if (!ValidEnumValue(tags[i])) continue;

                str.Append(tags[i]).Append(" = ").Append(1 << current);
                if (i < max - 1) {
                    str.Append(", ");
                }
                current++;
            }
            return str.ToString();
        }

        private static bool ValidEnumValue(string tag) => !tag.Contains(" ");

        [MenuItem("Assets/Update Tag Enum")]
        public static void Generate() {
            if (EditorUtility.DisplayDialog("Update Tag Enum", "This will create or overwrite the file SBR_Data/Tag.cs. This may produce some errors in the console. Don't worry about it.", "OK", "Cancel")) {
                DoGenerate();
            }
        }

        [MenuItem("Assets/Remove Tag Enum")]
        public static void Remove() {
            if (EditorUtility.DisplayDialog("Remove Tag Enum", "This will delete the generated Tags.cs file, and revert to using only the builtin tags.", "OK", "Cancel")) {
                DoRemove();
            }
        }

        private static void DoGenerate() {
            EditorPrefs.SetBool(checkTagsPref, true);
            didGenerate = true;
            string generated = string.Format(template, GetEnumValues());

            SBRProjectSettings.EnsureDataFolderExists();
            string defPath = Path.Combine(SBRProjectSettings.dataFolder, "Tag.cs");

            if (defPath.Length > 0) {
                string newPath = defPath.Substring(0, defPath.LastIndexOf(".")) + ".cs";

                StreamWriter outStream = new StreamWriter(newPath);
                outStream.Write(generated);
                outStream.Close();
                AssetDatabase.Refresh();
            }

            EditorUtil.SetSymbolDefined("TagsGenerated", true);
        }

        private static void DoRemove() {
            EditorPrefs.SetBool(checkTagsPref, false);
            string defPath = Path.Combine(SBRProjectSettings.dataFolder, "Tag.cs");
            if (File.Exists(defPath)) {
                AssetDatabase.DeleteAsset(defPath);
                AssetDatabase.Refresh();
            }
            EditorUtil.SetSymbolDefined("TagsGenerated", false);
        }
    }
}