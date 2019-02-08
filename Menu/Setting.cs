using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SBR.Menu {
    public abstract class Setting {
        public Setting(string key, Type settingType) {
            this.key = key;
            this.settingType = settingType;
        }

        public readonly string key;
        public readonly Type settingType;

        public string displayName {
            get {
                int i = key.LastIndexOf('/');
                if (i > 0 && i < key.Length - 2) {
                    return key.Substring(i + 1).SplitCamelCase();
                } else {
                    return null;
                }
            }
        }

        public event Action<object> ObjValueChanged;
        protected void OnObjValueChanged(object value) => ObjValueChanged?.Invoke(value);

        public abstract void Default();
        public abstract void Save();
        public abstract void Load();
        public abstract object objValue { get; set; }
        public abstract IEnumerable<object> objPossibleValues { get; }
        public abstract string ObjValueToString(object value);
    }

    public abstract class Setting<T> : Setting {
        private Func<T, string> toString;
        private T[] values;
        protected readonly T defaultValue;
        protected T currentValue;
        public event Action<T> ValueChanged;

        public Setting(
            string key, 
            T defaultValue = default, 
            Action<T> setter = null, 
            Func<T, string> toString = null, 
            T[] values = null) : 
            base(key, typeof(T)) {
            
            this.defaultValue = defaultValue;
            this.ValueChanged = setter;
            this.ValueChanged += v => OnObjValueChanged(v);
            this.toString = toString;
            this.values = values;

            currentValue = defaultValue;
        }

        public override object objValue { get => value; set => this.value = (T)value; }
        public T value {
            get => currentValue;
            set {
                if (!Equals(value, currentValue)) {
                    currentValue = value;
                    ValueChanged?.Invoke(value);
                }
            }
        }

        public override IEnumerable<object> objPossibleValues => values?.Cast<object>() ?? null;
        public virtual T[] possibleValues => values;
        public override string ObjValueToString(object value) => ValueToString((T)value);
        public virtual string ValueToString(T v) => toString?.Invoke(v) ?? v.ToString();
        public override void Default() => value = defaultValue;
    }

    public class IntSetting : Setting<int> {
        public IntSetting(string key, 
            int defaultValue = 0, 
            Action<int> setter = null, 
            Func<int, string> toString = null, 
            int[] values = null) : 
            base(key, defaultValue, setter, toString, values) { }

        public override void Load() => value = PlayerPrefs.GetInt(key, defaultValue);
        public override void Save() =>PlayerPrefs.SetInt(key, value);
    }

    public class EnumSetting<T> : Setting<T> where T : Enum {
        public EnumSetting(string key, 
            T defaultValue = default, 
            Action<T> setter = null, 
            Func<T, string> toString = null) : 
            base(key, defaultValue, setter, toString ?? CamelCaseToSplit, 
                Enum.GetValues(typeof(T)).Cast<T>().ToArray()) {}

        public override void Load() => value = (T)Enum.ToObject(typeof(T), 
            PlayerPrefs.GetInt(key, Convert.ToInt32(defaultValue)));
        public override void Save() => PlayerPrefs.SetInt(key, Convert.ToInt32(value));

        private static string CamelCaseToSplit(T val) => val.ToString().SplitCamelCase();
    }

    public class FloatSetting : Setting<float> {
        public FloatSetting(string key, 
            float defaultValue = 0, 
            Action<float> setter = null, 
            Func<float, string> toString = null, 
            float[] values = null) : 
            base(key, defaultValue, setter, toString, values) { }

        public override void Load() => value = PlayerPrefs.GetFloat(key, defaultValue);
        public override void Save() => PlayerPrefs.SetFloat(key, value);
    }

    public class BoolSetting : Setting<bool> {
        public BoolSetting(string key,
            bool defaultValue = false,
            Action<bool> setter = null,
            Func<bool, string> toString = null) :
            base(key, defaultValue, setter, toString ?? EnabledDisabled, null) { }

        public override void Load() => value = PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) == 1;
        public override void Save() => PlayerPrefs.SetInt(key, value ? 1 : 0);

        private static string EnabledDisabled(bool b) => b ? "Enabled" : "Disabled";
    }

    public class StringSetting : Setting<string> {
        public StringSetting(string key,
            string defaultValue = "",
            Action<string> setter = null,
            Func<string, string> toString = null,
            string[] values = null) :
            base(key, defaultValue, setter, toString, values) { }

        public override void Load() => value = PlayerPrefs.GetString(key, defaultValue);
        public override void Save() => PlayerPrefs.SetString(key, value);
    }
}

