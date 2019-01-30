using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable 649
namespace SBR {
    /// <summary>
    /// Used to provide additional parameters when drawing a DraggableList.
    /// It's use is optional, as the DraggableList works fine on its own.
    /// </summary>
    public class DraggableListDisplayAttribute : PropertyAttribute {
        /// <summary>
        /// Relative path to string or Object property to use for labels.
        /// </summary>
        public string labelProperty;

        /// <summary>
        /// Whether to show expander arrows on properties.
        /// </summary>
        public bool showExpanders;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="labelProperty">Relative path to string or Object property to use for labels.</param>
        /// <param name="showExpanders">Whether to show exapander arrows.</param>
        public DraggableListDisplayAttribute(string labelProperty = null, bool showExpanders = true) {
            this.labelProperty = labelProperty;
            this.showExpanders = showExpanders;
        }
    }

    /// <summary>
    /// Used to create a list that is shown in the inspector as a ReorderableList.
    /// Use to create a nicer interface than the default array view.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class DraggableList<T> : DoNotUse.DraggableList, IEnumerable<T> {
        /// <summary>
        /// Directly access the backing array.
        /// </summary>
        public T[] items;

        /// <summary>
        /// Get or set item at position in array.
        /// </summary>
        /// <param name="i">The index</param>
        /// <returns>The value at that index.</returns>
        public T this[int i] {
            get { return items[i]; }
            set { items[i] = value; }
        }

        /// <summary>
        /// Length of the array.
        /// </summary>
        public int length { get { return items.Length; } }

        /// <summary>
        /// Get a copy of the items array.
        /// </summary>
        public T[] itemsCopy { get { return (T[])items.Clone(); } }

        public IEnumerator<T> GetEnumerator() {
            return ((IEnumerable<T>)items).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return items.GetEnumerator();
        }
    }
}

namespace SBR.DoNotUse {
    [Serializable]
    public class DraggableList { }
}
