// MIT License
// 
// Copyright (c) 2020 Vincent Miller
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SBR {
    public enum SearchMode {
        Self, Parent, Children,
    }

    public struct AutoComponent<T> where T : Component {
        private T _cached;
        private bool _retry;
        private Component _owner;
        private bool _hasTried;
        private SearchMode _mode;

        public AutoComponent(SearchMode mode = SearchMode.Self, bool retry = true) {
            _owner = null;
            _cached = null;
            _retry = retry;
            _hasTried = true;
            _mode = mode;
        }

        public T Value(Component owner) {
            if (!_owner) {
                _owner = owner;
            }
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            else if (owner != _owner) {
                throw new UnityException("Cannot change owner of AutoComponent.");
            }
#endif

            if (!_cached && (!_hasTried || _retry)) {
                if (_mode == SearchMode.Self) {
                    _cached = _owner.GetComponent<T>();
                } else if (_mode == SearchMode.Parent) {
                    _cached = _owner.GetComponentInParent<T>();
                } else if (_mode == SearchMode.Children) {
                    _cached = _owner.GetComponentInChildren<T>();
                }
            }

            return _cached;
        }

        public void Reset() {
            _hasTried = false;
            _cached = null;
        }
    }
}
