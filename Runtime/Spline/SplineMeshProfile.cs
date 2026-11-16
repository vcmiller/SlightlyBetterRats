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
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace SBR {
    /// <summary>
    /// An asset used to control the appearance of a SplineMesh.
    /// </summary>
    [CreateAssetMenu(menuName = "Infohazard/Spline Mesh Profile")]
    public class SplineMeshProfile : ScriptableObject {
        /// <summary>
        /// Invoked when the profile changes and thus the mesh needs to update.
        /// </summary>
        public event Action<bool> PropertyChanged;

        /// <summary>
        /// Whether to build a separate mesh for the collision, or just use the render mesh.
        /// </summary>
        [FormerlySerializedAs("separateCollisionMesh"),Tooltip("Whether to build a separate mesh for the collision, or just use the render mesh.")]
        public bool _separateCollisionMesh;

        /// <summary>
        /// Whether to separate all materials in the MeshRenderer, or combine them all into one material slot.
        /// </summary>
        [FormerlySerializedAs("keepSeparateMaterials"),Tooltip("Whether to separate all materials in the MeshRenderer, or combine them all into one material slot.")]
        public bool _keepSeparateMaterials;

        /// <summary>
        /// The sequence of meshes that is repeated along the spline.
        /// </summary>
        [FormerlySerializedAs("meshes"),Tooltip("The sequence of meshes that is repeated along the spline.")]
        public MeshInfo[] _meshes = new MeshInfo[0];

        private int[] _submeshStartIndices;

        [Flags]
        public enum StretchMode {
            Nothing = 0,
            Gaps = 1,
            Mesh = 2
        }

        public enum AlignMode {
            Deform,
            Align,
            ForceUpright,
            NoRotation
        }

        /// <summary>
        /// A single mesh in the sequence.
        /// </summary>
        [Serializable]
        public class MeshInfo {
            /// <summary>
            /// Mesh used to create render mesh. Also used for collision if separateCollisionMesh is false.
            /// </summary>
            [FormerlySerializedAs("render"),Tooltip("Mesh used to create render mesh. Also used for collision if separateCollisionMesh is false.")]
            public Mesh _render;

            /// <summary>
            /// Mesh used for collision if separateCollisionMesh is true.
            /// </summary>
            [FormerlySerializedAs("collision"),Tooltip("Mesh used for collision if separateCollisionMesh is true.")]
            public Mesh _collision;

            /// <summary>
            /// Number of times to repeat the mesh before moving to the next.
            /// </summary>
            [FormerlySerializedAs("repeat"),Tooltip("Number of times to repeat the mesh before moving to the next.")]
            public int _repeat = 1;

            /// <summary>
            /// How the mesh is stretched along the spline in order to perfectly reach the end.
            /// </summary>
            [FormerlySerializedAs("stretchMode"),Tooltip("How the mesh is stretched along the spline in order to perfectly reach the end.")]
            public StretchMode _stretchMode = StretchMode.Nothing;

            /// <summary>
            /// How the mesh is aligned and deformed along the spline.
            /// </summary>
            [FormerlySerializedAs("alignMode"),Tooltip("How the mesh is aligned and deformed along the spline.")]
            public AlignMode _alignMode = AlignMode.Deform;

            /// <summary>
            /// Empty space before each occurance of the mesh.
            /// </summary>
            [FormerlySerializedAs("gapBefore"),Tooltip("Empty space before each occurance of the mesh.")]
            public float _gapBefore;

            /// <summary>
            /// Empty space after each occurance of the mesh.
            /// </summary>
            [FormerlySerializedAs("gapAfter"),Tooltip("Empty space after each occurance of the mesh.")]
            public float _gapAfter;

            /// <summary>
            /// Length of the mesh along the spline.
            /// </summary>
            public float MeshLength { get { return _render != null ? _render.bounds.size.z : 0; } }

            /// <summary>
            /// Length of the gaps along the spline.
            /// </summary>
            public float GapLength { get { return _gapBefore + _gapAfter; } }

            /// <summary>
            /// Length of mesh + gaps along the spline.
            /// </summary>
            public float TotalLength { get { return MeshLength + GapLength; } }
        }

        private void OnValidate() {
            if (!Application.isPlaying) {
                PropertyChanged?.Invoke(false);
            }
        }

        /// <summary>
        /// Create or update the rendering and collision mesh, and return them in out parameters.
        /// </summary>
        /// <param name="spline">The spline to align to.</param>
        /// <param name="mesh">The resulting render mesh.</param>
        /// <param name="collisionMesh">The resulting collision mesh.</param>
        public virtual void CreateMeshes(SplineData spline, out Mesh mesh, out Mesh collisionMesh) {
            if (_meshes.Length == 0) {
                Debug.LogError("SplineMeshProfile needs at least one mesh!");
            }

            int submeshCount = GetSubmeshCount();

            List<Vector3> vertices = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int>[] triangles = new List<int>[submeshCount];

            List<Vector3> collisionVertices = null;
            List<Vector3> collisionNormals = null;
            List<int> collisionTriangles = null;
            
            if (_separateCollisionMesh) {
                collisionVertices = new List<Vector3>();
                collisionNormals = new List<Vector3>();
                collisionTriangles = new List<int>();
            }

            if (_meshes.Any(m => m.TotalLength > 0)) {
                float f = 0;
                int meshIndex = 0;
                int rep = 0;

                float stretch = CalculateStretch(spline);

                while (f < 1) {
                    if (_meshes[meshIndex]._render) {
                        f = AddMesh(spline, meshIndex, stretch, f, vertices, normals, uvs, triangles, collisionVertices, collisionNormals, collisionTriangles);
                    } else {
                        f += _meshes[meshIndex].GapLength / spline.Length;
                    }

                    rep++;
                    if (rep >= _meshes[meshIndex]._repeat) {
                        rep = 0;
                        meshIndex = (meshIndex + 1) % _meshes.Length;
                    }
                }
            } else {
                Debug.LogWarning("SplineMesh cannot be built without at least one mesh with a z size greater than 0.");
            }

            mesh = new Mesh();
            mesh.name = "Spline Mesh";
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.subMeshCount = triangles.Length;
            for (int i = 0; i < triangles.Length; i++) {
                mesh.SetTriangles(triangles[i], i);
            }
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
            
            if (_separateCollisionMesh) {
                collisionMesh = new Mesh();
                collisionMesh.name = "Spline Collision";
                collisionMesh.SetVertices(collisionVertices);
                collisionMesh.SetNormals(collisionNormals);
                collisionMesh.subMeshCount = 1;
                collisionMesh.SetTriangles(collisionTriangles, 0);
                collisionMesh.RecalculateTangents();
                collisionMesh.RecalculateBounds();
            } else {
                collisionMesh = mesh;
            }
        }

        /// <summary>
        /// Get the number of submeshes that are used in the render mesh.
        /// </summary>
        /// <returns>The number of submeshes.</returns>
        public int GetSubmeshCount() {
            if (_keepSeparateMaterials) {
                int submeshCount = 0;
                if (_submeshStartIndices == null || _submeshStartIndices.Length != _meshes.Length) {
                    _submeshStartIndices = new int[_meshes.Length];
                }

                for (int i = 0; i < _meshes.Length; i++) {
                    _submeshStartIndices[i] = submeshCount;
                    if (_meshes[i]._render) {
                        submeshCount += _meshes[i]._render.subMeshCount;
                    }
                }

                return submeshCount;
            } else {
                return 1;
            }
        }

        protected virtual float CalculateStretch(SplineData spline) {
            float length = spline.Length;

            float meshLength = 0;
            float stretchable = 0;

            int meshIndex = 0;
            int rep = 0;
            while (true) {
                float sz = _meshes[meshIndex].MeshLength;
                float gap = _meshes[meshIndex].GapLength;

                if (meshLength + sz + gap <= length) {
                    meshLength += sz + gap;

                    if ((_meshes[meshIndex]._stretchMode & StretchMode.Mesh) == StretchMode.Mesh) {
                        stretchable += sz;
                    }

                    if ((_meshes[meshIndex]._stretchMode & StretchMode.Gaps) == StretchMode.Gaps) {
                        stretchable += gap;
                    }
                } else {
                    break;
                }

                rep++;
                if (rep >= _meshes[meshIndex]._repeat) {
                    rep = 0;
                    meshIndex = (meshIndex + 1) % _meshes.Length;
                }
            }

            if (stretchable > 0) {
                return (length - meshLength + stretchable) / stretchable;
            } else {
                return 1;
            }
        }

        protected virtual float AddMesh(SplineData spline, int meshIndex, float stretchFactor, float meshStart, 
            List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs, List<int>[] triangles,
            List<Vector3> collisionVertices, List<Vector3> collisionNormals, List<int> collisionTriangles) {
            MeshInfo mesh = _meshes[meshIndex];
            Mesh toAdd = mesh._render;
            Mesh toAddC = mesh._collision;

            StretchMode stretch = mesh._stretchMode;
            AlignMode align = mesh._alignMode;

            float sizeZ = mesh.MeshLength;
            float startGap = _meshes[meshIndex]._gapBefore;
            float endGap = _meshes[meshIndex]._gapAfter;

            if ((stretch & StretchMode.Mesh) == StretchMode.Mesh) {
                sizeZ *= stretchFactor;
            }

            if ((stretch & StretchMode.Gaps) == StretchMode.Gaps) {
                startGap *= stretchFactor;
                endGap *= stretchFactor;
            }

            meshStart += startGap / spline.Length;
            float meshEnd = meshStart + (sizeZ + endGap) / spline.Length;

            if (meshEnd <= 1 + (0.1f / spline.Length)) {
                int vCount = vertices.Count;
                float minZ = toAdd.bounds.min.z;
                Vector3 center = toAdd.bounds.center;
                AddVertexDataDeformed(spline, toAdd, stretch, align, stretchFactor, meshStart, minZ, center, vertices, normals, uvs);
            
                for (int i = 0; i < toAdd.subMeshCount; i++) {
                    int destList = 0;
                    if (_keepSeparateMaterials) {
                        destList = _submeshStartIndices[meshIndex] + i;
                    }

                    if (triangles[destList] == null) {
                        triangles[destList] = new List<int>();
                    }

                    AddTriangleData(toAdd, i, vCount, triangles[destList]);
                }

                if (_separateCollisionMesh && toAddC) {
                    int cvCount = collisionVertices.Count;
                    AddVertexDataDeformed(spline, toAddC, stretch, align, stretchFactor, meshStart, minZ, center, collisionVertices, collisionNormals, null);
                    AddTriangleData(toAddC, 0, cvCount, collisionTriangles);
                }
            }

            return meshEnd;
        }

        private void AddVertexDataDeformed(
            SplineData spline, Mesh mesh, StretchMode stretch, AlignMode align, 
            float stretchFactor, float meshStart, float minZ, Vector3 center,
            List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs) {
            
            Vector3[] inVerts = mesh.vertices;
            Vector3[] inNormals = mesh.normals;
            Vector2[] inUvs = uvs != null ? mesh.uv : null;
            
            float centerPos = Mathf.Clamp01(meshStart + (center.z - minZ) / spline.Length);
            Quaternion centerRotation = spline.GetRotation(centerPos);

            if (align == AlignMode.ForceUpright) {
                centerRotation = Quaternion.Euler(0, centerRotation.eulerAngles.y, 0);
            } else if (align == AlignMode.NoRotation) {
                centerRotation = Quaternion.identity;
            }

            Vector3 centerOffset = spline.GetPoint(centerPos) + (centerRotation * Vector3.forward * minZ);

            Matrix4x4 alignMatrix = Matrix4x4.TRS(centerOffset, centerRotation, Vector3.one);

            for (int i = 0; i < inVerts.Length; i++) {
                Vector3 vert = inVerts[i];
                Vector3 normal = inNormals[i];
                Vector2 uv = Vector2.zero;
                if (inUvs != null && inUvs.Length > i) {
                    uv = inUvs[i];
                }

                vert.z -= minZ;
                if ((stretch & StretchMode.Mesh) == StretchMode.Mesh) {
                    vert.z *= stretchFactor;
                }

                if (align == AlignMode.Deform) {
                    float pos = Mathf.Clamp01(meshStart + (vert.z / spline.Length));
                    Quaternion srot = spline.GetRotation(pos);
                    Vector3 spos = spline.GetPoint(pos);

                    vert = spos + (srot * Vector3.ProjectOnPlane(vert, Vector3.forward));
                    normal = srot * normal;
                } else {
                    vert = alignMatrix.MultiplyPoint(vert);

                    normal = alignMatrix.MultiplyVector(normal);
                }

                vertices.Add(vert);
                normals.Add(normal);
                if (uvs != null) uvs.Add(uv);
            }
        }

        private void AddTriangleData(Mesh mesh, int submesh, int vStart, List<int> triangles) {
            triangles.AddRange(mesh.GetTriangles(submesh).Select(t => t + vStart));
        }
    }
}
