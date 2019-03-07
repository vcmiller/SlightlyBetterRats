using System;
using UnityEngine;

/// <summary>
/// Attribute that tells Unity to only draw a property when a given condition is true.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property |
    AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]
public class ConditionalAttribute : PropertyAttribute {
    /// <summary>
    /// ConditionalAttribute that requires a serialized boolean field to be true.
    /// </summary>
    /// <param name="boolCondition">The name of a boolean field.</param>
    public ConditionalAttribute(string boolCondition) : this(boolCondition, true, true) { }

    /// <summary>
    /// ConditionalAttribute that requires a serialized field named condition to be equal or not equal to value.
    /// </summary>
    /// <param name="condition">The name of a field.</param>
    /// <param name="value">The value to compare to.</param>
    /// <param name="isEqual">Whether to check for equality or inequality.</param>
    public ConditionalAttribute(string condition, object value, bool isEqual = true) {
        this.condition = condition;
        this.value = value;
        this.isEqual = isEqual;
    }

    public readonly string condition;
    public readonly object value;
    public readonly bool isEqual;
}
