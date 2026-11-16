// The MIT License (MIT)
// 
// Copyright (c) 2026-present Val Miller
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
using System.IO;
using System.Linq;

namespace SBR.Editor {
    public static class ChannelsClassGenerator {

        private const string ClassTemplate = @"using UnityEngine;
using SBR;
using System.Collections.Generic;

public class {0} : {1} {{
{2}

    public override void ClearInput(bool force = false) {{
        base.ClearInput(force);
{3}
    }}

    public override void AddFields(Dictionary<string, object> values) {{
        base.AddFields(values);
{4}
    }}
}}
";

        private const string PropertyTemplate = @"
    private {0} _{1};
    public {0} {1} {{
        get {{ return _{1}; }}
        set {{
            _{1} = {2};
        }}
    }}
";

        private const string ClearTemplate = @"        _{0} = {1};
";

        private const string ClearTemplateIf = @"        if (force) _{0} = {1};
";

        private const string AddToDictionaryTemplate = @"        values[nameof({0})] = {0};
";

        public static void GenerateClass(ChannelsDefinition def) {
            string generated = string.Format(ClassTemplate, def.name, def._baseClass, GetProperties(def), GetClears(def), GetAdds(def));

            string defPath = AssetDatabase.GetAssetPath(def);

            if (defPath.Length > 0) {
                string newPath = defPath.Substring(0, defPath.LastIndexOf(".")) + ".cs";

                StreamWriter outStream = new StreamWriter(newPath);
                outStream.Write(generated);
                outStream.Close();
                AssetDatabase.Refresh();
            }
        }

        private static string GetClears(ChannelsDefinition def) {
            return string.Join("", def._channels.Select(GetClear));
        }

        private static string GetClear(ChannelsDefinition.Channel channel) {
            string template = channel._clears ? ClearTemplate : ClearTemplateIf;
            return string.Format(template, channel._name, GetChannelDefault(channel));
        }

        private static string GetAdds(ChannelsDefinition def) {
            return string.Join("", def._channels.Select(GetAdd));
        }

        private static string GetAdd(ChannelsDefinition.Channel channel) {
            return string.Format(AddToDictionaryTemplate, channel._name);
        }
        
        private static string GetChannelDefault(ChannelsDefinition.Channel def) {
            switch (def._type) {
                case ChannelsDefinition.ChannelType.Bool:
                    return def._defaultBool.ToString().ToLower();

                case ChannelsDefinition.ChannelType.Float:
                    return def._defaultFloat.ToString() + "f";

                case ChannelsDefinition.ChannelType.Int:
                    return def._defaultInt.ToString();

                case ChannelsDefinition.ChannelType.Vector:
                    Vector3 v = def._defaultVector;
                    return "new Vector3(" + v.x + "f, " + v.y + "f, " + v.z + "f)";

                case ChannelsDefinition.ChannelType.Quaternion:
                    Quaternion q = Quaternion.Euler(def._defaultRotation);
                    return "new Quaternion(" + q.x + "f, " + q.y + "f, " + q.z + "f, " + q.w + "f)";

                default:
                    return "default(" + GetType(def) + ")";
            }
        }

        private static string GetProperties(ChannelsDefinition def) {
            return string.Join("", def._channels.Select(c => GetProperty(c)));
        }

        private static string GetProperty(ChannelsDefinition.Channel channel) {
            return string.Format(PropertyTemplate, GetType(channel), channel._name, GetSetter(channel));
        }

        private static string GetType(ChannelsDefinition.Channel channel) {
            if (channel._type == ChannelsDefinition.ChannelType.Object) {
                if (channel._objectType.Length > 0) {
                    return channel._objectType;
                } else {
                    return "object";
                }
            } else if (channel._type == ChannelsDefinition.ChannelType.Quaternion) {
                return "Quaternion";
            } else if (channel._type == ChannelsDefinition.ChannelType.Vector) {
                return "Vector3";
            } else {
                return channel._type.ToString().ToLower();
            }
        }

        private static string GetSetter(ChannelsDefinition.Channel channel) {
            if (channel._type == ChannelsDefinition.ChannelType.Float && channel._floatHasRange) {
                return "Mathf.Clamp(value, " + channel._floatMin + "f, " + channel._floatMax + "f)";
            } else if (channel._type == ChannelsDefinition.ChannelType.Int && channel._intHasRange) {
                return "Mathf.Clamp(value, " + channel._intMin + ", " + channel._intMax + ")";
            } else if (channel._type == ChannelsDefinition.ChannelType.Vector && channel._vectorHasMax) {
                float sqr = channel._vectorMax * channel._vectorMax;
                return "value.sqrMagnitude > " + sqr + "f ? value.normalized * " + channel._vectorMax + "f : value";
            } else {
                return "value";
            }
        }
    }
}
