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
using System.Reflection;

namespace SBR.Menu {
    public static class SettingsManager {
        private static readonly Dictionary<string, ISetting> SettingsByName = new();

        public static IEnumerable<ISetting> AllSettings => SettingsByName.Values;
        public static IEnumerable<ISetting<T>> GetSettings<T>() => SettingsByName.Values.OfType<ISetting<T>>();
        public static IEnumerable<ISetting> GetSettings(Type t) => SettingsByName.Values.Where(s => s.SettingType == t);
        public static ISetting<T> GetSetting<T>(string key) => (ISetting<T>)GetSetting(key);
        public static ISetting GetSetting(string key) =>
            !string.IsNullOrEmpty(key) && SettingsByName.ContainsKey(key) ? SettingsByName[key] : null;

        public static void RegisterSettings(Type t) {
            foreach (FieldInfo field in t.GetFields(BindingFlags.Public | BindingFlags.Static)) {
                if (!typeof(ISetting).IsAssignableFrom(field.FieldType)) continue;

                ISetting setting = (ISetting)field.GetValue(null);
                if (SettingsByName.ContainsKey(setting.Key)) continue;

                SettingsByName[setting.Key] = setting;
                setting.Load();
            }
        }
    }
}
