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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

namespace SBR.Editor {
    public static class ChannelsClassGenerator {

        private static string classTemplate = @"using UnityEngine;
using SBR;

public class {0} : {1} {{
{2}

    public override void ClearInput(bool force = false) {{
        base.ClearInput(force);
{3}
    }}
}}
";

        private static string propertyTemplate = @"
    private {0} _{1};
    public {0} {1} {{
        get {{ return _{1}; }}
        set {{
            _{1} = {2};
        }}
    }}
";

        private static string clearTemplate = @"        _{0} = {1};
";
        private static string clearTemplateIf = @"        if (force) _{0} = {1};
";

        public static void GenerateClass(ChannelsDefinition def) {
            string generated = string.Format(classTemplate, def.name, def.baseClass, GetProperties(def), GetClears(def));

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
            return string.Join("", def.channels.Select(c => GetClear(c)));
        }

        private static string GetClear(ChannelsDefinition.Channel channel) {
            string template = channel.clears ? clearTemplate : clearTemplateIf;
            return string.Format(template, channel.name, GetChannelDefault(channel));
        }
        
        private static string GetChannelDefault(ChannelsDefinition.Channel def) {
            switch (def.type) {
                case ChannelsDefinition.ChannelType.Bool:
                    return def.defaultBool.ToString().ToLower();

                case ChannelsDefinition.ChannelType.Float:
                    return def.defaultFloat.ToString() + "f";

                case ChannelsDefinition.ChannelType.Int:
                    return def.defaultInt.ToString();

                case ChannelsDefinition.ChannelType.Vector:
                    Vector3 v = def.defaultVector;
                    return "new Vector3(" + v.x + "f, " + v.y + "f, " + v.z + "f)";

                case ChannelsDefinition.ChannelType.Quaternion:
                    Quaternion q = Quaternion.Euler(def.defaultRotation);
                    return "new Quaternion(" + q.x + "f, " + q.y + "f, " + q.z + "f, " + q.w + "f)";

                default:
                    return "default(" + GetType(def) + ")";
            }
        }

        private static string GetProperties(ChannelsDefinition def) {
            return string.Join("", def.channels.Select(c => GetProperty(c)));
        }

        private static string GetProperty(ChannelsDefinition.Channel channel) {
            return string.Format(propertyTemplate, GetType(channel), channel.name, GetSetter(channel));
        }

        private static string GetType(ChannelsDefinition.Channel channel) {
            if (channel.type == ChannelsDefinition.ChannelType.Object) {
                if (channel.objectType.Length > 0) {
                    return channel.objectType;
                } else {
                    return "object";
                }
            } else if (channel.type == ChannelsDefinition.ChannelType.Quaternion) {
                return "Quaternion";
            } else if (channel.type == ChannelsDefinition.ChannelType.Vector) {
                return "Vector3";
            } else {
                return channel.type.ToString().ToLower();
            }
        }

        private static string GetSetter(ChannelsDefinition.Channel channel) {
            if (channel.type == ChannelsDefinition.ChannelType.Float && channel.floatHasRange) {
                return "Mathf.Clamp(value, " + channel.floatMin + "f, " + channel.floatMax + "f)";
            } else if (channel.type == ChannelsDefinition.ChannelType.Int && channel.intHasRange) {
                return "Mathf.Clamp(value, " + channel.intMin + ", " + channel.intMax + ")";
            } else if (channel.type == ChannelsDefinition.ChannelType.Vector && channel.vectorHasMax) {
                float sqr = channel.vectorMax * channel.vectorMax;
                return "value.sqrMagnitude > " + sqr + "f ? value.normalized * " + channel.vectorMax + "f : value";
            } else {
                return "value";
            }
        }
    }
}