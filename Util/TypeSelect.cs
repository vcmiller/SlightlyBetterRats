using UnityEngine;
using System;

namespace SBR {
    /// <summary>
    /// Attribute that is applied to string fields to draw them as a dropdown where a Type is selected.
    /// </summary>
    public class TypeSelectAttribute : PropertyAttribute {
        public Type baseClass;
        public bool allowAbstract;

        /// <summary>
        /// Attribute that is applied to string fields to draw them as a dropdown where a Type is selected.
        /// </summary>
        /// <param name="baseClass">Base type from which types are selected.</param>
        /// <param name="allowAbstract">Can abstract classes be selected?</param>
        public TypeSelectAttribute(Type baseClass, bool allowAbstract = false) {
            this.baseClass = baseClass;
            this.allowAbstract = allowAbstract;
        }
    }
}