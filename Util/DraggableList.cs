using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable 649
namespace SBR {
    /// <summary>
    /// Used to create a list that is shown in the inspector as a ReorderableList.
    /// Use to create a nicer interface than the default array view.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class DraggableList<T> : DoNotUse.DraggableList, IEnumerable<T> {
        [SerializeField]
        private T[] items;

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
        /// A reference to the array used to store items.
        /// </summary>
        public T[] array { get { return items; } }

        /// <summary>
        /// Get a copy of the items array.
        /// </summary>
        public T[] arrayCopy { get { return (T[])items.Clone(); } }

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