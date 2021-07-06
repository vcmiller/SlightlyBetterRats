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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SBR.StateSystem;
using System.Linq;
using System;

namespace SBR {
    [ExecuteAlways]
    public class SwappableMesh : MonoBehaviour {
        private Animator _meshAnimator;
        private GameObject _meshInstance;

        [SerializeField, NoOverrides]
        private GameObject _meshPrefab;

        [SerializeField, NoOverrides]
        private Animator _useAnimator;
        
        public delegate void SwappableMeshChangingListener(GameObject oldPrefab, GameObject oldMesh, GameObject newPrefab, GameObject newMesh);
        public delegate void SwappableMeshChangedListener(GameObject newPrefab, GameObject newMesh);

        public event SwappableMeshChangingListener MeshChanging;
        public event SwappableMeshChangedListener MeshChanged;

        public GameObject MeshPrefab {
            get => _meshPrefab;
            set {
                if (value != _meshPrefab) {
                    UpdateMeshPrefab(value);
                }
            }
        }

        public GameObject MeshInstance => _meshInstance;

        [ContextMenu("Refresh Model")]
        public void RefreshModel() {
            UpdateMeshPrefab(_meshPrefab);
        }

        private void UpdateMeshPrefab(GameObject newPrefab) {
            GameObject oldPrefab = _meshPrefab;
            _meshPrefab = newPrefab;

            if (!_meshInstance) {
                transform.DestroyChildrenImmediate();
            }

            GameObject newMesh = CreateMesh();

            MeshChanging?.Invoke(oldPrefab, _meshInstance, newPrefab, newMesh);
            if (Application.isPlaying) {
                gameObject.SendMessageUpwards("OnSwappableMeshChanging", newMesh, SendMessageOptions.DontRequireReceiver);
            }

            if (_meshInstance) {
                DestroyImmediate(_meshInstance);
            }

            if (Application.isPlaying) {
                _meshInstance = newMesh;
                if (_useAnimator) {
                    TransferAnimatorFromInstance();
                } else {
                    _meshAnimator = newMesh.GetComponent<Animator>();
                }
            }

            MeshChanged?.Invoke(newPrefab, newMesh);
            if (Application.isPlaying) {
                gameObject.SendMessageUpwards("OnSwappableMeshChanged", newMesh, SendMessageOptions.DontRequireReceiver);
            }
        }

        private GameObject CreateMesh() {
            if (!_meshPrefab) return null;
            
            GameObject newInstance = Instantiate(_meshPrefab, transform, false);
            newInstance.name = "Swappable Mesh Instance";

            if (!Application.isPlaying) {
                newInstance.hideFlags = HideFlags.HideAndDontSave;
            }
                
            return newInstance;
        }

        private void TransferAnimatorFromInstance() {
            if (_meshInstance.TryGetComponent(out Animator instAnim)) {
                Animator myAnim = _useAnimator;

                if (!instAnim.avatar) {
                    Debug.LogError($"{instAnim} needs to have an avatar set.");
                }

                myAnim.avatar = instAnim.avatar;

                if (!myAnim.runtimeAnimatorController) {
                    myAnim.runtimeAnimatorController = instAnim.runtimeAnimatorController;
                }

                _meshAnimator = myAnim;

                DestroyImmediate(instAnim);
            } else {
                Debug.LogError($"{_meshPrefab} doesn't have an Animator on the root GameObject.");
            }
        }

        private void Awake() {
            if (Application.isPlaying) {
                UpdateMeshPrefab(_meshPrefab);
            }
        }

#if UNITY_EDITOR
        [NonSerialized]
        private GameObject _lastMeshPrefab;

        private void Reset() {
            _useAnimator = GetComponent<Animator>();
            Update();
        }

        private void Update() {
            if (Application.isPlaying) return;
            if (_lastMeshPrefab == _meshPrefab) return;
            
            GameObject newPrefab = _meshPrefab;
            _meshPrefab = _lastMeshPrefab;
            UpdateMeshPrefab(newPrefab);
            _lastMeshPrefab = _meshPrefab;
        }
#endif
    }
}