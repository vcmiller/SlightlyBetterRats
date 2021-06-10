using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SBR {
    public class ExpandableAttribute : PropertyAttribute {
        public bool AlwaysExpanded { get; }
        public Type CreatedType { get; }
        public string SavePath { get; }
        
        public ExpandableAttribute(bool alwaysExpanded = false, Type createdType = null, string savePath = null) {
            AlwaysExpanded = alwaysExpanded;
            CreatedType = createdType;
            SavePath = savePath;
        }
    }
}