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
using System.Linq;
using UnityEngine;

namespace SBR.Menu {
    public class SettingsSubMenu : MonoBehaviour {
        private Setting[] settings;

        public bool modified => settings.Any(s => s.modified);

        private void Awake() {
            settings = GetComponentsInChildren<SettingControl>().Select(t => t.setting).ToArray();
        }

        public void Defaults() {
            foreach (var setting in settings) {
                SettingsManager.GetSetting(setting.key).Default();
            }
        }

        public void Revert() {
            foreach (var setting in settings) {
                SettingsManager.GetSetting(setting.key).Load();
            }
        }

        public void Save() {
            foreach (var setting in settings) {
                SettingsManager.GetSetting(setting.key).Save();
            }
        }
    }

}