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
        
        public string name {
            get {
                int i = key.LastIndexOf('/');
                if (i > 0 && i < key.Length - 2) {
                    return key.Substring(i + 1);
                } else {
                    return null;
                }
            }
        }

        public bool modified { get; protected set; }
        public string displayName => name?.SplitCamelCase();

        public event Action<object> ObjValueChanged;
        protected void OnObjValueChanged(object value) => ObjValueChanged?.Invoke(value);

        public abstract void Default();
        public virtual void Save() => modified = false;
        public virtual void Load() => modified = false;
        public abstract object objValue { get; set; }
        public abstract IEnumerable<object> objPossibleValues { get; }
        public abstract string ObjValueToString(object value);
    }

    public abstract class Setting<T> : Setting {
        private Func<T, string> toString;
        private T[] values;
        private bool hasDefault;
        protected readonly T defaultValue;
        protected T currentValue;
        public event Action<T> ValueChanged;

        public Setting(
            string key, 
            T defaultValue = default, 
            bool hasDefault = true,
            Action<T> setter = null, 
            Func<T, string> toString = null, 
            T[] values = null) : 
            base(key, typeof(T)) {
            
            this.defaultValue = defaultValue;
            this.hasDefault = hasDefault;
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
                Debug.Log(key + ": " + value);
                if (!Equals(value, currentValue)) {
                    currentValue = value;
                    modified = true;
                    ValueChanged?.Invoke(value);
                }
            }
        }

        public override IEnumerable<object> objPossibleValues => values?.Cast<object>() ?? null;
        public virtual T[] possibleValues => values;
        public override string ObjValueToString(object value) => ValueToString((T)value);
        public virtual string ValueToString(T v) => toString?.Invoke(v) ?? v.ToString();
        public override void Default() {
            if (hasDefault) value = defaultValue;
        }
    }

    public class IntSetting : Setting<int> {
        public IntSetting(string key, 
            int defaultValue = 0,
            bool hasDefault = true,
            Action<int> setter = null, 
            Func<int, string> toString = null, 
            int[] values = null) : 
            base(key, defaultValue, hasDefault, setter, toString, values) { }

        public override void Load() {
            value = PlayerPrefs.GetInt(key, defaultValue);
            base.Load();
        }
        public override void Save() {
            PlayerPrefs.SetInt(key, value);
            base.Save();
        }
    }

    public class EnumSetting<T> : Setting<T> where T : Enum {
        public EnumSetting(string key, 
            T defaultValue = default,
            bool hasDefault = true,
            Action<T> setter = null, 
            Func<T, string> toString = null) : 
            base(key, defaultValue, hasDefault, setter, toString ?? CamelCaseToSplit, 
                Enum.GetValues(typeof(T)).Cast<T>().ToArray()) {}

        public override void Load() {
            value = (T)Enum.ToObject(typeof(T), PlayerPrefs.GetInt(key, Convert.ToInt32(defaultValue)));
            base.Load();
        }

        public override void Save() {
            PlayerPrefs.SetInt(key, Convert.ToInt32(value));
            base.Save();
        }

        private static string CamelCaseToSplit(T val) => val.ToString().SplitCamelCase();
    }

    public class FloatSetting : Setting<float> {
        public FloatSetting(string key, 
            float defaultValue = 0,
            bool hasDefault = true,
            Action<float> setter = null, 
            Func<float, string> toString = null, 
            float[] values = null) : 
            base(key, defaultValue, hasDefault, setter, toString, values) { }

        public override void Load() {
            value = PlayerPrefs.GetFloat(key, defaultValue);
            base.Load();
        }

        public override void Save() {
            PlayerPrefs.SetFloat(key, value);
            base.Save();
        }
    }

    public class BoolSetting : Setting<bool> {
        public BoolSetting(string key,
            bool defaultValue = false,
            bool hasDefault = true,
            Action<bool> setter = null,
            Func<bool, string> toString = null) :
            base(key, defaultValue, hasDefault, setter, toString ?? EnabledDisabled, null) { }

        public override void Load() {
            value = PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) == 1;
            base.Load();
        }

        public override void Save() {
            PlayerPrefs.SetInt(key, value ? 1 : 0);
            base.Save();
        }

        private static string EnabledDisabled(bool b) => b ? "Enabled" : "Disabled";
    }

    public class StringSetting : Setting<string> {
        public StringSetting(string key,
            string defaultValue = "",
            bool hasDefault = true,
            Action<string> setter = null,
            Func<string, string> toString = null,
            string[] values = null) :
            base(key, defaultValue, hasDefault, setter, toString, values) { }

        public override void Load() {
            value = PlayerPrefs.GetString(key, defaultValue);
            base.Load();
        }

        public override void Save() {
            PlayerPrefs.SetString(key, value);
            base.Save();
        }
    }
}

