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
using UnityEngine;

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
        public Spline spline;

        /// <summary>
        /// The profile that controls the base meshes.
        /// </summary>
        public SplineMeshProfile profile;

        /// <summary>
        /// Whether to generate the mesh in Awake(). Only needed if instantiating at runtime.
        /// </summary>
        public bool updateMeshOnAwake;
        
        public MeshFilter[] filters;
        public MeshCollider[] colliders;

        private Mesh _ownedMesh, _ownedCollision;
        public Mesh ownedMesh => _ownedMesh;
        public Mesh ownedCollision => _ownedCollision;

        private bool needsUpdate = false;

        private SplineMeshProfile _subscribedProfile;
        private SplineMeshProfile subscribedProfile {
            get { return _subscribedProfile; }
            set {
                if (_subscribedProfile) _subscribedProfile.PropertyChanged -= MarkDirty;
                _subscribedProfile = value;
                if (_subscribedProfile) _subscribedProfile.PropertyChanged += MarkDirty;
            }
        }

        private Spline _subscribedSpline;
        private Spline subscribedSpline {
            get { return _subscribedSpline; }
            set {
                if (_subscribedSpline) _subscribedSpline.PropertyChanged -= MarkDirty;
                _subscribedSpline = value;
                if (_subscribedSpline) _subscribedSpline.PropertyChanged += MarkDirty;
            }
        }

        private void Awake() {
            if (updateMeshOnAwake) {
                MarkDirty(true);
            }
        }

        private void OnEnable() {
            subscribedProfile = profile;
            subscribedSpline = spline;
        }

        private void OnDisable() {
            subscribedProfile = null;
            subscribedSpline = null;
        }

        private void OnValidate() {
            if (!Application.isPlaying && enabled) {
                OnEnable();
                MarkDirty(false);
            }
        }

        private void Reset() {
            spline = GetComponent<Spline>();
            
            var mf = GetComponent<MeshFilter>();
            var mc = GetComponent<MeshCollider>();
            
            if (mf) filters = new[] { mf };
            if (mc) colliders = new[] { mc };
        }
        
        private void MarkDirty(bool update) {
            needsUpdate = true;

            if (update) {
                Update();
            }
        }

        private void Update() {
            if (needsUpdate && enabled) {
                needsUpdate = false;
                UpdateMesh();
            }
        }

        /// <summary>
        /// Rebuild the mesh.
        /// </summary>
        public void UpdateMesh() {
            if (!this) {
                subscribedProfile = null;
                subscribedSpline = null;
                return;
            }

            if (ownedMesh) DestroyImmediate(ownedMesh);
            if (ownedCollision) DestroyImmediate(ownedCollision);

            if (profile && spline.spline.points.Length > 1) {
                profile.CreateMeshes(spline.spline, out _ownedMesh, out _ownedCollision);

#if UNITY_EDITOR
                if (gameObject.isStatic)
                    UnityEditor.Unwrapping.GenerateSecondaryUVSet(ownedMesh);
#endif

                foreach (var mf in filters) {
                    mf.sharedMesh = ownedMesh;
                    var mr = mf.GetComponent<MeshRenderer>();
                    var sm = mr.sharedMaterials;
                    int smc = profile.GetSubmeshCount();
                    if (sm.Length != profile.GetSubmeshCount()) {
                        Array.Resize(ref sm, smc);
                        mr.sharedMaterials = sm;
                    }
                }

                foreach (var mc in colliders) {
                    mc.sharedMesh = ownedCollision;
                }
            }
        }
    }
}