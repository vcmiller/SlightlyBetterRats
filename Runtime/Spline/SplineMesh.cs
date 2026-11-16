// The MIT License (MIT)
// 
// Copyright (c) 2026-present Val Miller
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
using UnityEngine;
using UnityEngine.Serialization;

namespace SBR {
    /// <summary>
    /// Component that constructs a mesh by duplicating and deforming a sequence of base meshes along a spline.
    /// Controlled by a SplineMeshProfile.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
    public class SplineMesh : MonoBehaviour {
        /// <summary>
        /// The spline that controls the shape.
        /// </summary>
        [FormerlySerializedAs("spline")]
        public Spline _spline;

        /// <summary>
        /// The profile that controls the base meshes.
        /// </summary>
        [FormerlySerializedAs("profile")]
        public SplineMeshProfile _profile;

        /// <summary>
        /// Whether to generate the mesh in Awake(). Only needed if instantiating at runtime.
        /// </summary>
        [FormerlySerializedAs("updateMeshOnAwake")]
        public bool _updateMeshOnAwake;
        
        [FormerlySerializedAs("filters")]
        public MeshFilter[] _filters;
        [FormerlySerializedAs("colliders")]
        public MeshCollider[] _colliders;

        private Mesh _ownedMesh, _ownedCollision;
        public Mesh OwnedMesh => _ownedMesh;
        public Mesh OwnedCollision => _ownedCollision;

        private bool _needsUpdate = false;

        private SplineMeshProfile _subscribedProfile;
        private SplineMeshProfile SubscribedProfile {
            get { return _subscribedProfile; }
            set {
                if (_subscribedProfile) _subscribedProfile.PropertyChanged -= MarkDirty;
                _subscribedProfile = value;
                if (_subscribedProfile) _subscribedProfile.PropertyChanged += MarkDirty;
            }
        }

        private Spline _subscribedSpline;
        private Spline SubscribedSpline {
            get { return _subscribedSpline; }
            set {
                if (_subscribedSpline) _subscribedSpline.PropertyChanged -= MarkDirty;
                _subscribedSpline = value;
                if (_subscribedSpline) _subscribedSpline.PropertyChanged += MarkDirty;
            }
        }

        private void Awake() {
            if (_updateMeshOnAwake) {
                MarkDirty(true);
            }
        }

        private void OnEnable() {
            SubscribedProfile = _profile;
            SubscribedSpline = _spline;
        }

        private void OnDisable() {
            SubscribedProfile = null;
            SubscribedSpline = null;
        }

        private void OnValidate() {
            if (!Application.isPlaying && enabled) {
                OnEnable();
                MarkDirty(false);
            }
        }

        private void Reset() {
            _spline = GetComponent<Spline>();
            
            MeshFilter mf = GetComponent<MeshFilter>();
            MeshCollider mc = GetComponent<MeshCollider>();
            
            if (mf) _filters = new[] { mf };
            if (mc) _colliders = new[] { mc };
        }
        
        private void MarkDirty(bool update) {
            _needsUpdate = true;

            if (update) {
                Update();
            }
        }

        private void Update() {
            if (_needsUpdate && enabled) {
                _needsUpdate = false;
                UpdateMesh();
            }
        }

        /// <summary>
        /// Rebuild the mesh.
        /// </summary>
        public void UpdateMesh() {
            if (!this) {
                SubscribedProfile = null;
                SubscribedSpline = null;
                return;
            }

            if (OwnedMesh) DestroyImmediate(OwnedMesh);
            if (OwnedCollision) DestroyImmediate(OwnedCollision);

            if (_profile && _spline._spline._points.Length > 1) {
                _profile.CreateMeshes(_spline._spline, out _ownedMesh, out _ownedCollision);

#if UNITY_EDITOR
                if (gameObject.isStatic)
                    UnityEditor.Unwrapping.GenerateSecondaryUVSet(OwnedMesh);
#endif

                foreach (MeshFilter mf in _filters) {
                    mf.sharedMesh = OwnedMesh;
                    MeshRenderer mr = mf.GetComponent<MeshRenderer>();
                    Material[] sm = mr.sharedMaterials;
                    int smc = _profile.GetSubmeshCount();
                    if (sm.Length != _profile.GetSubmeshCount()) {
                        Array.Resize(ref sm, smc);
                        mr.sharedMaterials = sm;
                    }
                }

                foreach (MeshCollider mc in _colliders) {
                    mc.sharedMesh = OwnedCollision;
                }
            }
        }
    }
}
