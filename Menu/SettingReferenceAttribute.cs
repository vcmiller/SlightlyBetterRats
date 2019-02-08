using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SBR.Menu {
    public class SettingReferenceAttribute : PropertyAttribute {
        public readonly Type[] settingTypes;

        public SettingReferenceAttribute() {
            this.settingTypes = null;
        }

        public SettingReferenceAttribute(params Type[] settingTypes) {
            Array.Sort(settingTypes, new TypeArrayComparer());
            this.settingTypes = settingTypes;
        }

        private class TypeArrayComparer : IComparer<Type> {
            public int Compare(Type x, Type y) {
                return x.GUID.GetHashCode() - y.GUID.GetHashCode();
            }
        }
    }
}
