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
using UnityEngine;
using UnityEngine.Audio;

namespace SBR.Menu {
    public class AudioVolumeUpdater : MonoBehaviour {
        [Serializable]
        public class Info {
            [SettingReference(typeof(float))]
            public string setting;
            public AudioMixer mixer;
            public string nameOverride;

            [NonSerialized]
            public Action<float> listener;
        }

        public Info[] settings;

        private void OnEnable() {
            foreach (var item in settings) {
                var setting = SettingsManager.GetSetting<float>(item.setting);
                if (setting == null) continue;
                item.listener = v => UpdateSettings(item, setting, v);
                setting.ValueChanged += item.listener;
                UpdateSettings(item, setting, setting.value);
            }
        }

        private void Start() {
            foreach (var item in settings) {
                var setting = SettingsManager.GetSetting<float>(item.setting);
                if (setting == null) continue;
                UpdateSettings(item, setting, setting.value);
            }
        }

        private void OnDisable() {
            foreach (var item in settings) {
                var setting = SettingsManager.GetSetting<float>(item.setting);
                if (setting == null) continue;
                setting.ValueChanged -= item.listener;
            }
        }

        private void UpdateSettings(Info item, Setting setting, float val) {
            float log = Mathf.Log(Mathf.Clamp(val, 0.001f, 1.0f)) * 20;

            string name = item.nameOverride;
            if (string.IsNullOrEmpty(name)) {
                name = setting.name;
            }

            item.mixer.SetFloat(name, log);
        }
    }
}