using System;
using UnityEngine;

/// <summary>
/// Attribute that tells Unity to only draw a property when a given condition is true. The condition is the name of a parameterless function, field, or property of type bool.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property |
    AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]
public class ConditionalAttribute : PropertyAttribute {
    public ConditionalAttribute(string boolCond) : this(boolCond, true, true) { }
    public ConditionalAttribute(string boolCond, bool value) : this(boolCond, value, true) { }

    public ConditionalAttribute(string condition, object value, bool isEqual) {
        if (value.GetType().IsEnum) {
            value = (int)value;
        }

        this.condition = condition;
        this.value = value;
        this.isEqual = isEqual;
    }

    public readonly string condition;
    public readonly object value;
    public readonly bool isEqual;
}
