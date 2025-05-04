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
        private readonly List<T> _contents = new();
        private readonly List<T> _additions = new();
        private readonly List<T> _deletions = new();

        public void Update() {
            _contents.AddRange(_additions);

            foreach (T deletion in _deletions) {
                _contents.Remove(deletion);
            }

            _additions.Clear();
            _deletions.Clear();
        }

        public int Count => _contents.Count;
        public bool IsReadOnly => false;

        public bool Add(T item) {
            if (_deletions.Remove(item)) {
                return true;
            } else if (!_contents.Contains(item) && !_additions.Contains(item)) {
                _additions.Add(item);
                return true;
            }

            return false;
        }

        public bool Remove(T item) {
            if (_additions.Remove(item)) {
                return true;
            } else if (_contents.Contains(item) && !_deletions.Contains(item)) {
                _deletions.Add(item);
                return true;
            }

            return false;
        }

        public void Clear() {
            _additions.Clear();
            foreach (var item in _contents) {
                _deletions.Add(item);
            }
        }

        public bool Contains(T item) {
            if (_additions.Contains(item)) {
                return true;
            } else if (_contents.Contains(item) && !_deletions.Contains(item)) {
                return true;
            }

            return false;
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => _contents.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => (_contents as IEnumerable).GetEnumerator();

        public List<T>.Enumerator GetEnumerator() => _contents.GetEnumerator();
    }
}
