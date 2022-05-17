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

using UnityEngine;
using UnityEngine.Events;

namespace SBR {
    public class TriggerVolume : MonoBehaviour {
        [MultiEnum]
        public Tag tagFilter = Tag.Player;
        public bool hideOnPlay = true;

        public event Action<GameObject> TriggerEntered;
        public event Action<GameObject> TriggerExited;
        public event Action<GameObject> AllExited;

        [SerializeField]
        private Events events = default;

        [Serializable]
        public struct Events {
            public UnityEvent OnTriggerEnter;
            public UnityEvent OnTriggerExit;
            public UnityEvent OnAllExit;
        }

        private HashSet<GameObject> _objects = new HashSet<GameObject>();

        private List<GameObject> _objectsToRemove = new List<GameObject>();
        private void Update() {
            _objects.RemoveWhere(obj => !obj);
            
            foreach (GameObject obj in _objects) {
                if (!obj.activeInHierarchy) _objectsToRemove.Add(obj);
            }

            foreach (GameObject obj in _objectsToRemove) {
                HandleExit(obj);
            }
        }

        private void HandleEnter(GameObject other) {
            if (!enabled) return;
            if (!other.CompareTag(tagFilter)) return;
            if (!_objects.Add(other.gameObject)) return;
            
            events.OnTriggerEnter?.Invoke();
            TriggerEntered?.Invoke(other);
        }

        private void HandleExit(GameObject other) {
            if (!enabled) return;
            if (!other.CompareTag(tagFilter)) return;
            if (!_objects.Remove(other)) return;
            
            events.OnTriggerExit?.Invoke();
            TriggerExited?.Invoke(other.gameObject);
            if (_objects.Count == 0) {
                events.OnAllExit?.Invoke();
                AllExited?.Invoke(other.gameObject);
            }
        }

        private void OnTriggerEnter(Collider other) => HandleEnter(other.gameObject);

        private void OnTriggerExit(Collider other) => HandleExit(other.gameObject);

        private void OnTriggerEnter2D(Collider2D other) => HandleEnter(other.gameObject);

        private void OnTriggerExit2D(Collider2D other) => HandleExit(other.gameObject);
    }

}