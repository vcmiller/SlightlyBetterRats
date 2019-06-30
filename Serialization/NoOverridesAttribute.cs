using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SBR.Serialization {
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class NoOverridesAttribute : Attribute { }

}