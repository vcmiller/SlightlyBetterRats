﻿using UnityEngine;
using System;

namespace SBR {
    /// <summary>
    /// Attribute that is applied to string fields to draw them as a dropdown where a Type is selected.
    /// </summary>
    public class TypeSelectAttribute : PropertyAttribute {
        public readonly Type baseClass;
        public readonly bool allowAbstract;
        public readonly bool allowGeneric;
        public readonly bool search;

        /// <summary>
        /// Attribute that is applied to string fields to draw them as a dropdown where a Type is selected.
        /// </summary>
        /// <param name="baseClass">Base type from which types are selected.</param>
        /// <param name="allowAbstract">Can abstract classes be selected?</param>
        /// <param name="allowGeneric">Can generic classes be selected?</param>
        /// <param name="search">Should a search box be included/</param>
        public TypeSelectAttribute(Type baseClass, bool allowAbstract = false, bool allowGeneric = false, bool search = false) {
            this.baseClass = baseClass;
            this.allowAbstract = allowAbstract;
            this.allowGeneric = allowGeneric;
            this.search = search;
        }
    }
}