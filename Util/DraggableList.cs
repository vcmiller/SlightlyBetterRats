using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SBR {
    [Serializable]
    public class DraggableList<T> : DoNotUse.DraggableList, IEnumerable<T> {
        [SerializeField]
        private T[] items;

        public T this[int i] {
            get { return items[i]; }
            set { items[i] = value; }
        }

        public int length { get { return items.Length; } }

        public T[] array { get { return items; } }
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