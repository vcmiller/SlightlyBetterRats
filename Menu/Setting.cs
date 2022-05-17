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

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SBR.Menu {
    public abstract class Setting {
        public Setting(string key, Type settingType, string displayName, string description) {
            Key = key;
            SettingType = settingType;
            DisplayName = displayName;
            Description = description;
        }

        public string Key { get; }
        public Type SettingType { get; }
        public string DisplayName { get; }
        public string Description { get; }

        public bool Modified { get; protected set; }

        public event Action<object> ObjValueChanged;
        protected void OnObjValueChanged(object value) => ObjValueChanged?.Invoke(value);

        public abstract void Default();
        public virtual void Save() => Modified = false;
        public virtual void Load() => Modified = false;
        public abstract object ObjValue { get; set; }
        public abstract IEnumerable<object> ObjPossibleValues { get; }
        public abstract string ObjValueToString(object value);
    }

    public abstract class Setting<T> : Setting {
        private readonly Func<T, string> _toString;
        private readonly T[] _values;
        private readonly bool _hasDefault;
        protected T DefaultValue { get; }
        protected T CurrentValue { get; set; }
        public event Action<T> ValueChanged;

        public Setting(string key,
                       string displayName,
                       string description,
                       T defaultValue = default,
                       bool hasDefault = true,
                       Action<T> setter = null,
                       Func<T, string> toString = null,
                       T[] values = null) :
            base(key, typeof(T), displayName, description) {
            DefaultValue = defaultValue;
            _hasDefault = hasDefault;
            ValueChanged = setter;
            ValueChanged += v => OnObjValueChanged(v);
            _toString = toString;
            _values = values;

            CurrentValue = defaultValue;
        }

        public override object ObjValue {
            get => Value;
            set => this.Value = (T) value;
        }

        public T Value {
            get => CurrentValue;
            set {
                if (Equals(value, CurrentValue)) return;
                CurrentValue = value;
                Modified = true;
                ValueChanged?.Invoke(value);
            }
        }

        public override IEnumerable<object> ObjPossibleValues => _values?.Cast<object>() ?? null;
        public virtual T[] PossibleValues => _values;
        public override string ObjValueToString(object value) => ValueToString((T) value);
        public virtual string ValueToString(T v) => _toString?.Invoke(v) ?? v.ToString();

        public override void Default() {
            if (_hasDefault) Value = DefaultValue;
        }
    }

    public class IntSetting : Setting<int> {
        public IntSetting(string key,
                          string displayName,
                          string description,
                          int defaultValue = 0,
                          bool hasDefault = true,
                          Action<int> setter = null,
                          Func<int, string> toString = null,
                          int[] values = null) :
            base(key, displayName, description, defaultValue, hasDefault, setter, toString, values) { }

        public override void Load() {
            Value = PlayerPrefs.GetInt(Key, DefaultValue);
            base.Load();
        }

        public override void Save() {
            PlayerPrefs.SetInt(Key, Value);
            base.Save();
        }
    }

    public class EnumSetting<T> : Setting<T> where T : Enum {
        public EnumSetting(string key,
                           string displayName,
                           string description,
                           T defaultValue = default,
                           bool hasDefault = true,
                           Action<T> setter = null,
                           Func<T, string> toString = null) :
            base(key, displayName, description, defaultValue, hasDefault, setter, toString ?? CamelCaseToSplit,
                 Enum.GetValues(typeof(T)).Cast<T>().ToArray()) { }

        public override void Load() {
            Value = (T) Enum.ToObject(typeof(T), PlayerPrefs.GetInt(Key, Convert.ToInt32(DefaultValue)));
            base.Load();
        }

        public override void Save() {
            PlayerPrefs.SetInt(Key, Convert.ToInt32(Value));
            base.Save();
        }

        private static string CamelCaseToSplit(T val) => val.ToString().SplitCamelCase();
    }

    public class FloatSetting : Setting<float> {
        public FloatSetting(string key,
                            string displayName,
                            string description,
                            float defaultValue = 0,
                            bool hasDefault = true,
                            Action<float> setter = null,
                            Func<float, string> toString = null,
                            float[] values = null) :
            base(key, displayName, description, defaultValue, hasDefault, setter, toString, values) { }

        public override void Load() {
            Value = PlayerPrefs.GetFloat(Key, DefaultValue);
            base.Load();
        }

        public override void Save() {
            PlayerPrefs.SetFloat(Key, Value);
            base.Save();
        }
    }

    public class BoolSetting : Setting<bool> {
        public BoolSetting(string key,
                           string displayName,
                           string description,
                           bool defaultValue = false,
                           bool hasDefault = true,
                           Action<bool> setter = null,
                           Func<bool, string> toString = null) :
            base(key, displayName, description, defaultValue, hasDefault, setter, toString ?? EnabledDisabled, null) { }

        public override void Load() {
            Value = PlayerPrefs.GetInt(Key, DefaultValue ? 1 : 0) == 1;
            base.Load();
        }

        public override void Save() {
            PlayerPrefs.SetInt(Key, Value ? 1 : 0);
            base.Save();
        }

        private static string EnabledDisabled(bool b) => b ? "Enabled" : "Disabled";
    }

    public class StringSetting : Setting<string> {
        public StringSetting(string key,
                             string displayName,
                             string description,
                             string defaultValue = "",
                             bool hasDefault = true,
                             Action<string> setter = null,
                             Func<string, string> toString = null,
                             string[] values = null) :
            base(key, displayName, description, defaultValue, hasDefault, setter, toString, values) { }

        public override void Load() {
            Value = PlayerPrefs.GetString(Key, DefaultValue);
            base.Load();
        }

        public override void Save() {
            PlayerPrefs.SetString(Key, Value);
            base.Save();
        }
    }
}