// The MIT License (MIT)
// 
// Copyright (c) 2022-present Vincent Miller
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

namespace SBR.Menu {
    public static class SettingsManager {
        private static Dictionary<string, Setting> settingsByName = new Dictionary<string, Setting>();
        private static Dictionary<Type, List<Setting>> settingsByOwner = new Dictionary<Type, List<Setting>>();

        public static IEnumerable<Setting> allSettings => settingsByName.Values;
        public static IEnumerable<Setting<T>> GetSettings<T>() => settingsByName.Values.OfType<Setting<T>>();
        public static IEnumerable<Setting> GetSettings(Type t) => settingsByName.Values.Where(s => s.SettingType == t);
        public static Setting<T> GetSetting<T>(string key) => (Setting<T>)GetSetting(key);
        public static Setting GetSetting(string key) => 
            !string.IsNullOrEmpty(key) && settingsByName.ContainsKey(key) ? settingsByName[key] : null;

        public static IEnumerable<Type> owners => settingsByOwner.Keys;
        public static IEnumerable<Setting> GetSettingsForOwner(Type owner) {
            if (settingsByOwner.ContainsKey(owner)) {
                return settingsByOwner[owner].AsEnumerable();
            } else {
                return Enumerable.Empty<Setting>();
            }
        }


        public static void RegisterSettings(Type t) {
            if (!settingsByOwner.ContainsKey(t)) {
                settingsByOwner[t] = new List<Setting>();
                foreach (var field in t.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)) {
                    if (typeof(Setting).IsAssignableFrom(field.FieldType)) {
                        var setting = (Setting)field.GetValue(null);
                        if (!settingsByName.ContainsKey(setting.Key)) {
                            settingsByOwner[t] = new List<Setting>();
                            settingsByName[setting.Key] = setting;
                            setting.Load();
                        }
                    }
                }
            }
        }
    }
}