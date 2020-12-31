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
        private GameObject _meshInstance;
        private (Material[] mats, Mesh mesh, Matrix4x4 transform, Bounds bounds)[] _prefabMeshes;
        private Animator _meshAnimator;
        private Bounds _meshBounds;

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

        private void UpdatePrefabMeshes() {
            if (_meshPrefab) {
                _prefabMeshes = _meshPrefab.GetComponentsInChildren<MeshRenderer>()
                    .Select(mr => (mr, mf: mr.GetComponent<MeshFilter>()))
                    .Where(t => t.mf)
                    .Select(pair => (pair.mr.sharedMaterials, pair.mf.sharedMesh, pair.mr.transform.localToWorldMatrix, pair.mr.bounds))

                    .Concat(_meshPrefab.GetComponentsInChildren<SkinnedMeshRenderer>()
                    .Select(smr => (smr.sharedMaterials, smr.sharedMesh, smr.localToWorldMatrix, smr.bounds)))

                    .ToArray();
                _meshBounds = _prefabMeshes[0].bounds;
                foreach (var item in _prefabMeshes) {
                    _meshBounds.Encapsulate(item.bounds);
                }
            } else {
                _prefabMeshes = null;
            }
        }

        private void UpdateMeshPrefab(GameObject newPrefab) {
            GameObject oldPrefab = _meshPrefab;
            _meshPrefab = newPrefab;

            GameObject newMesh = CreateMesh();

            MeshChanging?.Invoke(oldPrefab, _meshInstance, newPrefab, newMesh);
            gameObject.SendMessageUpwards("OnSwappableMeshChanging", newMesh, SendMessageOptions.DontRequireReceiver);

            if (_meshInstance) {
                Destroy(_meshInstance);
            }

            _meshInstance = newMesh;

            if (_useAnimator) {
                TransferAnimatorFromInstance();
            } else {
                _meshAnimator = _meshInstance.GetComponent<Animator>();
            }

            MeshChanged?.Invoke(newPrefab, _meshInstance);
            gameObject.SendMessageUpwards("OnSwappableMeshChanged", newMesh, SendMessageOptions.DontRequireReceiver);
        }

        private GameObject CreateMesh() {
            if (_meshPrefab) {
                GameObject newInstance = Instantiate(_meshPrefab, transform, false);
                newInstance.name = "Swappable Mesh Instance";
                return newInstance;
            } else {
                return null;
            }
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

                Destroy(instAnim);
            } else {
                Debug.LogError($"{_meshPrefab} doesn't have an Animator on the root GameObject.");
            }
        }

        private Matrix4x4 GetTransformMatrix(Transform t) {
            if (t) {
                return t.localToWorldMatrix;
            } else {
                return Matrix4x4.identity;
            }
        }

        private Quaternion GetTransformRotation(Transform t) {
            if (t) {
                return t.rotation;
            } else {
                return Quaternion.identity;
            }
        }

        public Transform GetBoneTransform(HumanBodyBones bone) {
            if (!_meshInstance) return transform;

            if (Application.isPlaying) {
                if (_meshAnimator) {
                    return _meshAnimator.GetBoneTransform(bone);
                } else {
                    Debug.LogError($"Trying to get bone transform of {ToString()}, which has no Animator.");
                    return transform;
                }
            } else {
                throw new Exception("Bone transform doesn't exist in edit mode. Use GetBoneWorldMatrix instead.");
            }
        }

        public Matrix4x4 GetBoneWorldMatrix(HumanBodyBones bone) {
            if (Application.isPlaying) {
                // Avoid extra matrix multiplication that would happen here if we always called GetBoneLocalMatrix.
                return GetTransformMatrix(GetBoneTransform(bone));
            } else {
                return transform.localToWorldMatrix * GetBoneLocalMatrix(bone);
            }
        }

        public Matrix4x4 GetBoneLocalMatrix(HumanBodyBones bone) {
            if (Application.isPlaying) {
                return transform.worldToLocalMatrix * GetTransformMatrix(GetBoneTransform(bone));
            } else {
                if (_meshPrefab == null) {
                    return Matrix4x4.identity;
                }

                if (_meshPrefab.TryGetComponent(out Animator anim)) {
                    return GetTransformMatrix(anim.GetBoneTransform(bone));
                } else {
                    Debug.LogError($"{_meshPrefab} doesn't have an Animator on the root GameObject.");
                    return Matrix4x4.identity;
                }
            }
        }

        public Vector3 GetBoneLocalScale(HumanBodyBones bone) {
            if (_meshPrefab == null) {
                return Vector3.one;
            }

            if (_meshPrefab.TryGetComponent(out Animator anim)) {
                return anim.GetBoneTransform(bone).lossyScale;
            } else {
                Debug.LogError($"{_meshPrefab} doesn't have an Animator on the root GameObject.");
                return Vector3.one;
            }
        }

        public Quaternion GetBoneWorldRotation(HumanBodyBones bone) {
            if (Application.isPlaying) {
                // Avoid extra quaternion multiplication that would happen here if we always called GetBoneLocalMatrix.
                return GetTransformRotation(GetBoneTransform(bone));
            } else {
                return transform.rotation * GetBoneLocalRotation(bone);
            }
        }

        public Quaternion GetBoneLocalRotation(HumanBodyBones bone) {
            if (Application.isPlaying) {
                return Quaternion.Inverse(transform.rotation) * GetTransformRotation(GetBoneTransform(bone));
            } else {
                if (_meshPrefab == null) {
                    return Quaternion.identity;
                }

                if (_meshPrefab.TryGetComponent(out Animator anim)) {
                    return GetTransformRotation(anim.GetBoneTransform(bone));
                } else {
                    Debug.LogError($"{_meshPrefab} doesn't have an Animator on the root GameObject.");
                    return Quaternion.identity;
                }
            }
        }

        // Start is called before the first frame update
        private void Start() {
            if (Application.isPlaying) {
                UpdateMeshPrefab(_meshPrefab);
            }
        }

        private void OnDestroy() {
            if (Application.isPlaying) {
                if (_meshInstance) {
                    Destroy(_meshInstance);
                }
            }
        }

#if UNITY_EDITOR
        [NonSerialized]
        private GameObject _lastMeshPrefab;

        public (Material[] mats, Mesh mesh, Matrix4x4 transform, Bounds bounds)[] PrefabMeshes => _prefabMeshes;
        public Bounds MeshBounds => _meshBounds;

        private void Reset() {
            _useAnimator = GetComponent<Animator>();
            OnValidate();
        }

        private void OnValidate() {
            if (!Application.isPlaying) {
                if (_lastMeshPrefab != _meshPrefab) {
                    UpdatePrefabMeshes();
                    MeshChanging?.Invoke(_lastMeshPrefab, null, _meshPrefab, null);
                    MeshChanged?.Invoke(_meshPrefab, null);
                    _lastMeshPrefab = _meshPrefab;
                }
            }
        }

        private void OnEnable() {
            if (!Application.isPlaying) {
                Camera.onPreCull -= DrawMeshPreview;
                Camera.onPreCull += DrawMeshPreview;
            }
        }

        private void OnDisable() {
            if (!Application.isPlaying) {
                Camera.onPreCull -= DrawMeshPreview;
            }
        }

        private void OnDrawGizmos() {
            if (_prefabMeshes == null) return;

            var col = Color.clear;
            Gizmos.matrix = gameObject.transform.localToWorldMatrix;
            Gizmos.color = col;
            Gizmos.DrawCube(_meshBounds.center, _meshBounds.size);
        }

        private void DrawMeshPreview(Camera camera) {
            if (_prefabMeshes == null || camera.name == "Preview Scene Camera") return;
            if (UnityEditor.Experimental.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage() != null &&
                UnityEditor.Experimental.SceneManagement.PrefabStageUtility.GetPrefabStage(gameObject) == null) return;

            Matrix4x4 selfToWorld = transform.localToWorldMatrix;
            foreach ((Material[] mats, Mesh mesh, Matrix4x4 matrix, Bounds bounds) in _prefabMeshes) {
                Matrix4x4 prefabToWorld = selfToWorld * matrix;
                for (int i = 0; i < mats.Length && i < mesh.subMeshCount; i++) {
                    Graphics.DrawMesh(mesh, prefabToWorld, mats[i], 1, camera, i);
                }
            }
        }
#endif
    }
}