// The MIT License (MIT)
// 
// Copyright (c) 2022-present Vincent Miller
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System.Collections;
using System.Collections.Generic;

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
