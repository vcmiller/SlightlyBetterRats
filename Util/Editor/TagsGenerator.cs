using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Linq;
using System.IO;
using System.Text;
using System;

namespace SBR.Editor {
    [InitializeOnLoad]
    public static class TagsGenerator {
        private static readonly string folder = "Assets/SBR_Data/";
        private static readonly string template = @"using System;

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
            if (didGenerate || !EditorPrefs.GetBool("CheckTags", true)) {
                return;
            }
            var tags = InternalEditorUtility.tags;

            bool needsRegen;
            if (tags.Length != tagsValues.Length) {
                needsRegen = true;
            } else {
                needsRegen = !Enumerable.SequenceEqual(tags, tagsValues);
            }

            if (needsRegen) {
                if (EditorUtility.DisplayDialog("Generate Tags", "Do you want to generate a Tag.cs file?", "OK", "No")) {
                    DoGenerate();
                } else {
                    EditorPrefs.SetBool("CheckTags", false);
                }
            }
        }

        private static string GetEnumValues() {
            var tags = InternalEditorUtility.tags;
            var str = new StringBuilder();
            int max = Mathf.Min(tags.Length, 32);
            for (int i = 0; i < max; i++) {
                str.Append(tags[i]).Append(" = ").Append(1 << i);
                if (i < max - 1) {
                    str.Append(", ");
                }
            }
            return str.ToString();
        }

        [MenuItem("Assets/Update Tag Enum")]
        public static void Generate() {
            if (EditorUtility.DisplayDialog("Update Tag Enum", "This will create or overwrite the file SBR_Data/Tag.cs. This may produce some errors in the console. Don't worry about it.", "OK", "Cancel")) {
                DoGenerate();
            }
        }

        private static void DoGenerate() {
            EditorPrefs.SetBool("CheckTags", true);
            didGenerate = true;
            string generated = string.Format(template, GetEnumValues());

            Directory.CreateDirectory(folder);
            string defPath = folder + "Tag.cs";

            if (defPath.Length > 0) {
                string newPath = defPath.Substring(0, defPath.LastIndexOf(".")) + ".cs";

                StreamWriter outStream = new StreamWriter(newPath);
                outStream.Write(generated);
                outStream.Close();
                AssetDatabase.Refresh();
            }

            foreach (BuildTargetGroup value in Enum.GetValues(typeof(BuildTargetGroup))) {
                string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(value);
                if (!defines.Contains("TagsGenerated")) {
                    if (defines.Length == 0) {
                        defines = "TagsGenerated";
                    } else {
                        defines += ";TagsGenerated";
                    }

                    PlayerSettings.SetScriptingDefineSymbolsForGroup(value, defines);
                }
            }
        }
    }
}