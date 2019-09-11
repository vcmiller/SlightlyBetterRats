using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBR {
    public class SafeUpdateSet<T> : IEnumerable<T> {
        private HashSet<T> contents;
        private List<T> additions;
        private List<T> deletions;

        public SafeUpdateSet() {
            contents = new HashSet<T>();
            additions = new List<T>();
            deletions = new List<T>();
        }

        public void Update() {
            contents.UnionWith(additions);
            contents.ExceptWith(deletions);

            additions.Clear();
            deletions.Clear();
        }

        public int Count => contents.Count;
        public bool IsReadOnly => false;

        public bool Add(T item) {
            if (deletions.Remove(item)) {
                return true;
            } else if (!contents.Contains(item) && !additions.Contains(item)) {
                additions.Add(item);
                return true;
            }

            return false;
        }

        public bool Remove(T item) {
            if (additions.Remove(item)) {
                return true;
            } else if (contents.Contains(item) && !deletions.Contains(item)) {
                deletions.Add(item);
                return true;
            }

            return false;
        }

        public void Clear() {
            additions.Clear();
            foreach (var item in contents) {
                deletions.Add(item);
            }
        }

        public bool Contains(T item) {
            if (additions.Contains(item)) {
                return true;
            } else if (contents.Contains(item) && !deletions.Contains(item)) {
                return true;
            }

            return false;
        }
        
        public IEnumerator<T> GetEnumerator() => contents.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => (contents as IEnumerable).GetEnumerator();
    }
}
