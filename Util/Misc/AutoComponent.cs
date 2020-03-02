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
