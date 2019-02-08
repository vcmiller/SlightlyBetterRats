using System;
using System.Collections.Generic;
using System.Linq;

namespace SBR.Menu {
    public static class SettingsManager {
        private static Dictionary<string, Setting> settingsByName = new Dictionary<string, Setting>();
        private static Dictionary<Type, List<Setting>> settingsByOwner = new Dictionary<Type, List<Setting>>();

        public static IEnumerable<Setting> allSettings => settingsByName.Values;
        public static IEnumerable<Setting<T>> GetSettings<T>() => settingsByName.Values.OfType<Setting<T>>();
        public static IEnumerable<Setting> GetSettings(Type t) => settingsByName.Values.Where(s => s.settingType == t);
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
                        if (!settingsByName.ContainsKey(setting.key)) {
                            settingsByOwner[t] = new List<Setting>();
                            settingsByName[setting.key] = setting;
                            setting.Load();
                        }
                    }
                }
            }
        }
    }
}