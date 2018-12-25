using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SBR {
    /// <summary>
    /// Component that constructs a mesh by duplicating and deforming a sequence of base meshes along a spline.
    /// Controlled by a SplineMeshProfile.
    /// </summary>
    [ExecuteInEditMode]
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

        private Mesh ownedMesh;
        private Mesh ownedCollision;

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

        private MeshRenderer mr;
        private MeshFilter mf;
        private MeshCollider mc;

        private void OnEnable() {
            subscribedProfile = profile;
            subscribedSpline = spline;
        }

        private void OnDisable() {
            subscribedProfile = null;
            subscribedSpline = null;
        }

        private void OnValidate() {
            if (!Application.isPlaying) {
                OnEnable();
                MarkDirty(false);
            }
        }

        private void Reset() {
            spline = GetComponent<Spline>();
        }
        
        private void MarkDirty(bool update) {
            needsUpdate = true;

            if (update) {
                Update();
            }
        }

        private void Update() {
            if (needsUpdate) {
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

            mr = GetComponent<MeshRenderer>();
            mf = GetComponent<MeshFilter>();
            mc = GetComponent<MeshCollider>();

            if (ownedMesh) DestroyImmediate(ownedMesh);
            if (ownedCollision) DestroyImmediate(ownedCollision);

            if (profile && spline.spline.points.Length > 1) {
                profile.CreateMeshes(spline.spline, out ownedMesh, out ownedCollision);

                if (gameObject.isStatic)
#if UNITY_EDITOR
                    UnityEditor.Unwrapping.GenerateSecondaryUVSet(ownedMesh);
#endif

                mf.sharedMesh = ownedMesh;
                if (mc) mc.sharedMesh = ownedCollision;

                var sm = mr.sharedMaterials;
                int smc = profile.GetSubmeshCount();
                if (sm.Length != profile.GetSubmeshCount()) {
                    Array.Resize(ref sm, smc);
                    mr.sharedMaterials = sm;
                }
            }
        }
    }
}