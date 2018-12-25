using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace SBR {
    /// <summary>
    /// Attribute that allows selecting multiple Enum values.
    /// The enum declaration snould have the [Flags] attribute,
    /// and have its integer values set to avoid collisions.
    /// </summary>
    public class MultiEnumAttribute : PropertyAttribute { }
}
