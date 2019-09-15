using SBR.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SBR {
    [ExecuteAlways]
    public class AttachToBone : MonoBehaviour {
        [SerializeField, NoOverrides]
        private bool _useBoneScale;

        [SerializeField, NoOverrides]
        private HumanBodyBones _boneParent;

        [SerializeField, NoOverrides]
        private Vector3 _localPosition = Vector3.zero;

        [SerializeField, NoOverrides]
        [EulerAngles]
        private Quaternion _localRotation = Quaternion.identity;

        [SerializeField, NoOverrides]
        private Vector3 _localScale = Vector3.one;

        [NonSerialized]
        private SwappableMesh _swappableMesh;

        private LastTransform _lastTransform;

        public SwappableMesh Mesh {
            get => _swappableMesh;
            private set {
                if (value != _swappableMesh) {
                    if (_swappableMesh) {
                        _swappableMesh.MeshChanging -= MeshChanging;
                        _swappableMesh.MeshChanged -= MeshChanged;
                    }

                    _swappableMesh = value;

                    if (_swappableMesh) {
                        _swappableMesh.MeshChanging += MeshChanging;
                        _swappableMesh.MeshChanged += MeshChanged;
                    }

                    UpdateTransformFromValues();
                }
            }
        }

        public HumanBodyBones BoneParent {
            get => _boneParent;
            set {
                _boneParent = value;
                UpdateTransformFromValues();
            }
        }

        public bool UseBoneScale {
            get => _useBoneScale;
            set {
                _useBoneScale = value;
                UpdateTransformFromValues();
            }
        }

        public Vector3 LocalPosition {
            get => _localPosition;
            set {
                _localPosition = value;
                UpdateTransformFromValues();
            }
        }

        public Vector3 LocalEulerAngles {
            get => _localRotation.eulerAngles;
            set {
                _localRotation = Quaternion.Euler(value);
                UpdateTransformFromValues();
            }
        }

        public Quaternion LocalRotation {
            get => _localRotation;
            set {
                _localRotation = value;
                UpdateTransformFromValues();
            }
        }

        public Vector3 LocalScale {
            get => _localScale;
            set {
                _localScale = value;
                UpdateTransformFromValues();
            }
        }

        public void UpdateTransformFromValues() {
            if (!_swappableMesh) return;

            Vector3 localPositionBoneSpace = _localPosition;
            if (!_useBoneScale) {
                localPositionBoneSpace = MathUtil.Divide(localPositionBoneSpace, _swappableMesh.GetBoneLocalScale(BoneParent));
            }

            if (Application.isPlaying) {
                Transform boneTransform = _swappableMesh.GetBoneTransform(_boneParent);

                if (!_useBoneScale) {
                    transform.parent = _swappableMesh.transform;
                    transform.localScale = _localScale;
                }

                transform.parent = boneTransform;
                transform.localPosition = localPositionBoneSpace;
                transform.localRotation = _localRotation;

                if (_useBoneScale) {
                    transform.localScale = _localScale;
                }
            } else {
                transform.position = _swappableMesh.GetBoneWorldMatrix(_boneParent).MultiplyPoint(localPositionBoneSpace);
                transform.rotation = _swappableMesh.GetBoneWorldRotation(_boneParent) * _localRotation;
                transform.localScale = _localScale; // Can't multiply scale from matrix.
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(transform);
#endif
            }

            _lastTransform.Update();
        }

        private void UpdateValuesFromTransform() {
            if (!_swappableMesh) return;

            if (_lastTransform.LocalPositionChanged) {
                if (Application.isPlaying) {
                    _localPosition = transform.localPosition;
                } else {
                    _localPosition = _swappableMesh.GetBoneWorldMatrix(_boneParent).inverse.MultiplyPoint(transform.position);
                }
            }

            if (_lastTransform.LocalRotationChanged) {
                if (Application.isPlaying) {
                    _localRotation = transform.localRotation;
                } else {
                    _localRotation = Quaternion.Inverse(_swappableMesh.GetBoneWorldRotation(_boneParent)) * transform.rotation;
                }
            }

            if (_lastTransform.LocalScaleChanged) {
                _localScale = MathUtil.Multiply(_localScale, _lastTransform.ScaleRatio);
            }

            _lastTransform.Update();


#if UNITY_EDITOR
            if (!Application.isPlaying) {
                UnityEditor.EditorUtility.SetDirty(transform);
            }
#endif
        }

        private void MeshChanging(GameObject oldPrefab, GameObject oldMesh, GameObject newPrefab, GameObject newMesh) {
            if (Application.isPlaying) {
                // To make sure this object doesn't get destroyed along with the old mesh.
                transform.SetParent(_swappableMesh.transform, false);
            }

            // Humanoid rigs might have different bone rotations.
            // We can try to compensate for this by calculating the rotation offset,
            // and applying it to the local position and rotation.
            if (oldPrefab && newPrefab) {
                Transform oldParentBone;
                Transform newParentBone;

                if (oldPrefab.TryGetComponent(out Animator oldAnim) && newPrefab.TryGetComponent(out Animator newAnim) &&
                    (oldParentBone = oldAnim.GetBoneTransform(_boneParent)) && (newParentBone = newAnim.GetBoneTransform(_boneParent))) {

                    Quaternion rotationOffset = Quaternion.Inverse(newParentBone.rotation) * oldParentBone.rotation;

                    // There are two reasons the bones might have different rotations.
                    // 1. The primary axis orientations are different. The difference will (hopefully) be a multiple of 90 on each axis. We ignore this difference.
                    // 2. The rigs have different rest poses. In these cases, the difference will (hopefully) be less than 45 degrees.
                    // There's no good way to guarantee local transforms are maintained properly when changing rigs, but this is a best guess.
                    rotationOffset = Quaternion.Euler(MathUtil.RoundToNearest(rotationOffset.eulerAngles, 90));
                    //_localPosition = rotationOffset * _localPosition;
                    //_localRotation = rotationOffset * _localRotation;
                }
            }
        }

        private void MeshChanged(GameObject newPrefab, GameObject newMesh) {
            UpdateTransformFromValues();
        }

        private void OnEnable() {
            _lastTransform = new LastTransform(transform);
            Mesh = GetComponentInParent<SwappableMesh>();
        }

        private void Update() {
            if (!Application.isPlaying) {
                Mesh = GetComponentInParent<SwappableMesh>();
            }

            if (_lastTransform.LocalChanged) {
                UpdateValuesFromTransform();
            }
        }

        private void OnDestroy() {
            if (_swappableMesh) {
                _swappableMesh.MeshChanging -= MeshChanging;
                _swappableMesh.MeshChanged -= MeshChanged;
            }
        }
    }

}