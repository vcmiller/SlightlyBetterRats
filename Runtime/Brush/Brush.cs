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
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace SBR {

    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshCollider))]
    public class Brush : MonoBehaviour {
        [FormerlySerializedAs("type")]
        public Type _type;
        [FormerlySerializedAs("size")]
        public Vector3 _size = new Vector3(1, 1, 1);
        [FormerlySerializedAs("textureWorldScale")]
        public Vector3 _textureWorldScale = new Vector3(1, 1, 1);
        [FormerlySerializedAs("textureLocalScale")]
        public Vector2 _textureLocalScale = new Vector2(1, 1);
        [FormerlySerializedAs("complexity")]
        public int _complexity = 16;
        [FormerlySerializedAs("smooth")]
        public bool _smooth = false;
        [FormerlySerializedAs("stepSeparation")]
        public float _stepSeparation = 0.2f;
        [FormerlySerializedAs("stepHeight")]
        public float _stepHeight = 0.1f;
        [FormerlySerializedAs("addBottomStep")]
        public bool _addBottomStep = true;
        [FormerlySerializedAs("mesh")]
        public FreeformMesh _mesh;

        public MeshFilter Filter => GetComponent<MeshFilter>();
        public MeshRenderer Render => GetComponent<MeshRenderer>();
        public MeshCollider Col => GetComponent<MeshCollider>();

        [FormerlySerializedAs("undone")]
        [HideInInspector]
        public bool _undone = false;

        [HideInInspector]
        [NonSerialized]
        public bool Dirty;

        private Type _lastType = Type.Freeform;
        private Vector3 _lastSize;
        private Vector3 _lastWorldScale;
        private Vector2 _lastLocalScale;
        private int _lastComplexity;
        private bool _lastSmooth;
        private float _lastStepSep;
        private float _lastStepHeight;
        private bool _lastBottomStep;
        private bool _lastIsStatic;

        private void Start() {
            if (Application.isPlaying) {
                Destroy(this);
            } else {
                Filter.sharedMesh = new Mesh();
                Filter.sharedMesh.name = "Brush Mesh";
                Col.sharedMesh = Filter.sharedMesh;
                Dirty = true;
            }
        }

        public void Update() {

            if (!Dirty && !_undone &&
                (_lastType == _type &&
                _lastSize == _size &&
                _lastWorldScale == _textureWorldScale &&
                _lastLocalScale == _textureLocalScale &&
                _lastComplexity == _complexity &&
                _lastSmooth == _smooth &&
                _lastStepSep == _stepSeparation &&
                _lastStepHeight == _stepHeight &&
                _lastBottomStep == _addBottomStep &&
                _lastIsStatic == gameObject.isStatic)) {
                return;
            }

            Mesh m = Filter.sharedMesh;

            if (m == null) {
                m = new Mesh();
                m.name = "Brush Mesh";
            }

            List<Vector3> vertices = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<Vector3> normals = new List<Vector3>();
            List<List<int>> indices = new List<List<int>>();

            if (_type == Type.Box) {
                UpdateBox(vertices, uvs, indices);
                Col.convex = true;
            } else if (_type == Type.Slant) {
                UpdateSlant(vertices, uvs, indices);
                Col.convex = true;
            } else if (_type == Type.Cyllinder) {
                UpdateCyllinder(vertices, uvs, normals, indices);
                Col.convex = true;
            } else if (_type == Type.BlockStair) {
                UpdateBlockStair(vertices, uvs, indices);
            } else if (_type == Type.SlantStair) {
                UpdateSlantStair(vertices, uvs, indices);
            } else if (_type == Type.SeparateStair) {
                UpdateSeparateStair(vertices, uvs, indices);
            } else {
                if (_lastType != Type.Freeform || _mesh == null) {
                    _mesh = new FreeformMesh();

                    UpdateBox(vertices, uvs, indices);
                }

                UpdateFreeform(vertices, uvs, indices);
            }

            m.subMeshCount = 0;
            m.vertices = null;
            m.uv = null;

            m.vertices = vertices.ToArray();
            m.uv = uvs.ToArray();
            m.subMeshCount = indices.Count;

            if (_type == Type.Cyllinder && _smooth) {
                m.normals = normals.ToArray();
            }

            for (int i = 0; i < indices.Count; i++) {
                m.SetIndices(indices[i].ToArray(), MeshTopology.Triangles, i);
            }

            m.RecalculateBounds();

            if (_type != Type.Cyllinder || !_smooth) {
                m.RecalculateNormals();
            }

            if (gameObject.isStatic)
#if UNITY_EDITOR
                UnityEditor.Unwrapping.GenerateSecondaryUVSet(m);
#endif

            Filter.sharedMesh = m;
            Col.sharedMesh = m;

            Material[] mats = Render.sharedMaterials;
            if (m.subMeshCount != mats.Length && m.subMeshCount != 0) {
                Material[] newMats = new Material[m.subMeshCount];

                for (int i = 0; i < newMats.Length; i++) {
                    newMats[i] = mats[Mathf.Min(i, mats.Length - 1)];
                }

                Render.sharedMaterials = newMats;
            }

            if (_undone) {
                _mesh.ForceGraphUpdate();
            }

            _undone = false;
            Dirty = false;
            _lastType = _type;
            _lastSize = _size;
            _lastWorldScale = _textureWorldScale;
            _lastLocalScale = _textureLocalScale;
            _lastComplexity = _complexity;
            _lastSmooth = _smooth;
            _lastStepSep = _stepSeparation;
            _lastStepHeight = _stepHeight;
            _lastBottomStep = _addBottomStep;
            _lastIsStatic = gameObject.isStatic;
        }

        private void UpdateFreeform(List<Vector3> vertices, List<Vector2> uvs, List<List<int>> indices) {
            if (_mesh == null) {
                _mesh = new FreeformMesh();
            }

            indices.Add(new List<int>());

            _mesh.GetMeshData(vertices, uvs, indices[0], _textureWorldScale, _textureLocalScale);
        }

        private void UpdateBlockStair(List<Vector3> vertices, List<Vector2> uvs, List<List<int>> indices) {
            if (_mesh == null) {
                _mesh = new FreeformMesh();
            } else {
                _mesh.Clear();
            }

            int numSteps = Mathf.CeilToInt(_size.y / _stepSeparation);

            float stepLength = _size.z / numSteps;

            int last1 = -1, last2 = -1, back1 = -1, back2 = -1;

            for (int i = 0; i < numSteps; i++) {
                float shAct = _size.y / numSteps;
                float height = _size.y - (i * shAct);
                int v1 = i > 0 ? last1 : _mesh.AddVertex(new Vector3(0, height, i * stepLength));
                int v2 = i > 0 ? last2 : _mesh.AddVertex(new Vector3(_size.x, height, i * stepLength));
                int v3 = _mesh.AddVertex(new Vector3(_size.x, height, (i + 1) * stepLength));
                int v4 = _mesh.AddVertex(new Vector3(0, height, (i + 1) * stepLength));

                int v5 = _mesh.AddVertex(new Vector3(0, height - shAct, (i + 1) * stepLength));
                int v6 = _mesh.AddVertex(new Vector3(_size.x, height - shAct, (i + 1) * stepLength));

                int v7 = _mesh.AddVertex(new Vector3(0, 0, i * stepLength));
                int v8 = _mesh.AddVertex(new Vector3(_size.x, 0, i * stepLength));
                int v9 = _mesh.AddVertex(new Vector3(_size.x, 0, (i + 1) * stepLength));
                int v10 = _mesh.AddVertex(new Vector3(0, 0, (i + 1) * stepLength));

                _mesh.AddFace(v4, v3, v2, v1); // Step
                _mesh.AddFace(v3, v4, v5, v6); // Back
                _mesh.AddFace(v7, v10, v4, v1);
                _mesh.AddFace(v9, v8, v2, v3);

                if (i == 0) {
                    _mesh.AddFace(v8, v7, v1, v2);
                    back1 = v7;
                    back2 = v8;
                } else if (i == numSteps - 1) {
                    _mesh.AddFace(v9, v10, back1, back2);
                }

                last1 = v5;
                last2 = v6;
            }

            if (_addBottomStep) {
                int v1 = _mesh.AddVertex(new Vector3(0, 0, _size.z + stepLength));
                int v2 = _mesh.AddVertex(new Vector3(_size.x, 0, _size.z + stepLength));

                _mesh.AddFace(v1, v2, v2, v1);
            }

            indices.Add(new List<int>());

            _mesh.GetMeshData(vertices, uvs, indices[0], _textureWorldScale, _textureLocalScale);
        }

        private void UpdateSlantStair(List<Vector3> vertices, List<Vector2> uvs, List<List<int>> indices) {

            if (_mesh == null) {
                _mesh = new FreeformMesh();
            } else {
                _mesh.Clear();
            }

            int numSteps = Mathf.CeilToInt(_size.y / _stepSeparation);

            float stepLength = _size.z / numSteps;

            int last1 = -1, last2 = -1, back1 = -1, back2 = -1;

            for (int i = 0; i < numSteps; i++) {
                float shAct = _size.y / numSteps;
                float height = _size.y - (i * shAct);
                int v1 = i > 0 ? last1 : _mesh.AddVertex(new Vector3(0, height, i * stepLength));
                int v2 = i > 0 ? last2 : _mesh.AddVertex(new Vector3(_size.x, height, i * stepLength));
                int v3 = _mesh.AddVertex(new Vector3(_size.x, height, (i + 1) * stepLength));
                int v4 = _mesh.AddVertex(new Vector3(0, height, (i + 1) * stepLength));

                int v5 = _mesh.AddVertex(new Vector3(0, height - shAct, (i + 1) * stepLength));
                int v6 = _mesh.AddVertex(new Vector3(_size.x, height - shAct, (i + 1) * stepLength));

                int v7 = _mesh.AddVertex(new Vector3(0, i > numSteps - 2 ? 0 : height - 2 * shAct, i * stepLength));
                int v8 = _mesh.AddVertex(new Vector3(_size.x, i > numSteps - 2 ? 0 : height - 2 * shAct, i * stepLength));
                int v9 = _mesh.AddVertex(new Vector3(_size.x, i > numSteps - 3 ? 0 : height - 3 * shAct, (i + 1) * stepLength));
                int v10 = _mesh.AddVertex(new Vector3(0, i > numSteps - 3 ? 0 : height - 3 * shAct, (i + 1) * stepLength));

                _mesh.AddFace(v4, v3, v2, v1); // Step
                _mesh.AddFace(v3, v4, v5, v6); // Back
                _mesh.AddFace(v7, v10, v4, v1);
                _mesh.AddFace(v9, v8, v2, v3);

                if (i == 0) {
                    _mesh.AddFace(v8, v7, v1, v2);
                    back1 = v7;
                    back2 = v8;
                } else if (i == numSteps - 2) {
                    _mesh.AddFace(v8, v7, back1, back2);
                    back1 = v7;
                    back2 = v8;
                } else if (i == numSteps - 1) {
                    _mesh.AddFace(v9, v10, back1, back2);
                }

                last1 = v5;
                last2 = v6;
            }

            if (_addBottomStep) {
                int v1 = _mesh.AddVertex(new Vector3(0, 0, _size.z + stepLength));
                int v2 = _mesh.AddVertex(new Vector3(_size.x, 0, _size.z + stepLength));

                _mesh.AddFace(v1, v2, v2, v1);
            }

            indices.Add(new List<int>());

            _mesh.GetMeshData(vertices, uvs, indices[0], _textureWorldScale, _textureLocalScale);
        }

        private void UpdateSeparateStair(List<Vector3> vertices, List<Vector2> uvs, List<List<int>> indices) {

            if (_mesh == null) {
                _mesh = new FreeformMesh();
            } else {
                _mesh.Clear();
            }

            int numSteps = Mathf.CeilToInt(_size.y / _stepSeparation);

            float stepLength = _size.z / numSteps;

            for (int i = 0; i < numSteps; i++) {
                float shAct = _size.y / numSteps;
                float height = _size.y - (i * shAct);
                int v1 = _mesh.AddVertex(new Vector3(0, height, i * stepLength));
                int v2 = _mesh.AddVertex(new Vector3(_size.x, height, i * stepLength));
                int v3 = _mesh.AddVertex(new Vector3(_size.x, height, (i + 1) * stepLength));
                int v4 = _mesh.AddVertex(new Vector3(0, height, (i + 1) * stepLength));

                int v7 = _mesh.AddVertex(new Vector3(0, height - _stepHeight, i * stepLength));
                int v8 = _mesh.AddVertex(new Vector3(_size.x, height - _stepHeight, i * stepLength));
                int v9 = _mesh.AddVertex(new Vector3(_size.x, height - _stepHeight, (i + 1) * stepLength));
                int v10 = _mesh.AddVertex(new Vector3(0, height - _stepHeight, (i + 1) * stepLength));

                _mesh.AddFace(v4, v3, v2, v1); // Step
                _mesh.AddFace(v3, v4, v10, v9); // Back
                _mesh.AddFace(v7, v10, v4, v1);
                _mesh.AddFace(v9, v8, v2, v3);
                _mesh.AddFace(v8, v7, v1, v2);
                _mesh.AddFace(v7, v8, v9, v10);
            }

            if (_addBottomStep) {
                int v1 = _mesh.AddVertex(new Vector3(0, 0, _size.z + stepLength));
                int v2 = _mesh.AddVertex(new Vector3(_size.x, 0, _size.z + stepLength));

                _mesh.AddFace(v1, v2, v2, v1);
            }

            indices.Add(new List<int>());

            _mesh.GetMeshData(vertices, uvs, indices[0], _textureWorldScale, _textureLocalScale);
        }

        private void UpdateBox(List<Vector3> vertices, List<Vector2> uvs, List<List<int>> indices) {

            if (_mesh == null) {
                _mesh = new FreeformMesh();
            } else {
                _mesh.Clear();
            }

            int v1 = _mesh.AddVertex(new Vector3(_size.x, _size.y, _size.z));
            int v2 = _mesh.AddVertex(new Vector3(0, _size.y, _size.z));
            int v3 = _mesh.AddVertex(new Vector3(0, _size.y, 0));
            int v4 = _mesh.AddVertex(new Vector3(_size.x, _size.y, 0));
            int v5 = _mesh.AddVertex(new Vector3(_size.x, 0, _size.z));
            int v6 = _mesh.AddVertex(new Vector3(0, 0, _size.z));
            int v7 = _mesh.AddVertex(new Vector3(0, 0, 0));
            int v8 = _mesh.AddVertex(new Vector3(_size.x, 0, 0));

            _mesh.AddFace(v2, v1, v4, v3);
            _mesh.AddFace(v1, v2, v6, v5);
            _mesh.AddFace(v3, v4, v8, v7);
            _mesh.AddFace(v2, v3, v7, v6);
            _mesh.AddFace(v4, v1, v5, v8);
            _mesh.AddFace(v7, v8, v5, v6);

            indices.Add(new List<int>());

            _mesh.GetMeshData(vertices, uvs, indices[0], _textureWorldScale, _textureLocalScale);
        }

        private void UpdateSlant(List<Vector3> vertices, List<Vector2> uvs, List<List<int>> indices) {

            if (_mesh == null) {
                _mesh = new FreeformMesh();
            } else {
                _mesh.Clear();
            }

            int v1 = _mesh.AddVertex(new Vector3(0, _size.y, 0));
            int v2 = _mesh.AddVertex(new Vector3(_size.x, _size.y, 0));
            int v3 = _mesh.AddVertex(new Vector3(_size.x, 0, _size.z));
            int v4 = _mesh.AddVertex(new Vector3(0, 0, _size.z));
            int v5 = _mesh.AddVertex(new Vector3(0, 0, 0));
            int v6 = _mesh.AddVertex(new Vector3(_size.x, 0, 0));

            _mesh.AddFace(v1, v2, v6, v5);
            _mesh.AddFace(v5, v6, v3, v4);
            _mesh.AddFace(v2, v1, v4, v3); // Slant
            _mesh.AddFace(v5, v4, v1);
            _mesh.AddFace(v6, v2, v3);

            indices.Add(new List<int>());

            _mesh.GetMeshData(vertices, uvs, indices[0], _textureWorldScale, _textureLocalScale);
        }

        private void UpdateCyllinder(List<Vector3> vertices, List<Vector2> uvs, List<Vector3> normals, List<List<int>> indices) {
            Vector3 ext = _size / 2;

            Vector3 offset = ext;

            Vector3 topCenter = new Vector3(0, ext.y, 0) + offset;
            Vector3 botCenter = new Vector3(0, -ext.y, 0) + offset;

            indices.Add(new List<int>());

            for (float i = 0; i < _complexity; i++) {
                float angle = Mathf.PI * 2 * (i / _complexity);
                float angle2 = Mathf.PI * 2 * ((i + 1) / _complexity);

                float sin = Mathf.Sin(angle);
                float cos = Mathf.Cos(angle);

                float sin2 = Mathf.Sin(angle2);
                float cos2 = Mathf.Cos(angle2);

                Vector3 v1 = new Vector3(sin * ext.x, -ext.y, cos * ext.z) + offset;
                Vector3 v2 = new Vector3(sin * ext.x, ext.y, cos * ext.z) + offset;
                Vector3 v3 = new Vector3(sin2 * ext.x, ext.y, cos2 * ext.z) + offset;
                Vector3 v4 = new Vector3(sin2 * ext.x, -ext.y, cos2 * ext.z) + offset;

                float texrep = 4 * Mathf.Sqrt(_size.x * _size.z / (_textureWorldScale.x * _textureWorldScale.z));
                float txInc = texrep / _complexity;
                float tx = texrep - (txInc * i);

                Vector2 tw = new Vector3(ext.x / _textureWorldScale.x, ext.z / _textureWorldScale.z);

                Tri(v2, v3, topCenter,
                    new Vector2((sin + 1) * tw.x, (cos + 1) * tw.y),
                    new Vector2((sin2 + 1) * tw.x, (cos2 + 1) * tw.y),
                    tw, 1, 1, vertices, uvs, indices[0]);

                for (int j = 0; j < 3; j++) {
                    normals.Add(Vector3.up);
                }

                CylQuad(v3, v2, v1, v4, tx - txInc, tx, 1, _size.y / _textureWorldScale.y, vertices, uvs, indices[0]);

                normals.Add(new Vector3(sin2, 0, cos2));
                normals.Add(new Vector3(sin, 0, cos));
                normals.Add(new Vector3(sin, 0, cos));
                normals.Add(new Vector3(sin2, 0, cos2));

                Tri(v4, v1, botCenter,
                    new Vector2((-sin2 + 1) * tw.x, (cos2 + 1) * tw.y),
                    new Vector2((-sin + 1) * tw.x, (cos + 1) * tw.y),
                    tw, 1, 1, vertices, uvs, indices[0]);

                for (int j = 0; j < 3; j++) {
                    normals.Add(Vector3.down);
                }
            }
        }

        private void Tri(Vector3 v1, Vector3 v2, Vector3 v3, Vector2 t1, Vector2 t2, Vector2 t3, float xscale, float yscale, List<Vector3> vertices, List<Vector2> uvs, List<int> indices) {
            xscale /= _textureLocalScale.x;
            yscale /= _textureLocalScale.y;

            vertices.Add(v1);
            uvs.Add(new Vector2(t1.x * xscale, t1.y * yscale));
            indices.Add(vertices.Count - 1);

            vertices.Add(v2);
            uvs.Add(new Vector2(t2.x * xscale, t2.y * yscale));
            indices.Add(vertices.Count - 1);

            vertices.Add(v3);
            uvs.Add(new Vector2(t3.x * xscale, t3.y * yscale));
            indices.Add(vertices.Count - 1);
        }

        private void CylQuad(
            Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, float t1X, float t2X, float xscale, float yscale, List<Vector3> vertices, List<Vector2> uvs, List<int> indices) {

            xscale /= _textureLocalScale.x;
            yscale /= _textureLocalScale.y;

            vertices.Add(v1);
            uvs.Add(new Vector2(t1X * xscale, yscale));
            indices.Add(vertices.Count - 1);

            vertices.Add(v2);
            uvs.Add(new Vector2(t2X * xscale, yscale));
            indices.Add(vertices.Count - 1);

            vertices.Add(v3);
            uvs.Add(new Vector2(t2X * xscale, 0));
            indices.Add(vertices.Count - 1);
            indices.Add(vertices.Count - 1);

            vertices.Add(v4);
            uvs.Add(new Vector2(t1X * xscale, 0));
            indices.Add(vertices.Count - 1);
            indices.Add(vertices.Count - 4);
        }

        private void Quad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, float xscale, float yscale, List<Vector3> vertices, List<Vector2> uvs, List<int> indices) {
            xscale /= _textureLocalScale.x;
            yscale /= _textureLocalScale.y;

            vertices.Add(v1);
            uvs.Add(new Vector2(0, yscale));
            indices.Add(vertices.Count - 1);

            vertices.Add(v2);
            uvs.Add(new Vector2(xscale, yscale));
            indices.Add(vertices.Count - 1);

            vertices.Add(v3);
            uvs.Add(new Vector2(xscale, 0));
            indices.Add(vertices.Count - 1);
            indices.Add(vertices.Count - 1);

            vertices.Add(v4);
            uvs.Add(new Vector2(0, 0));
            indices.Add(vertices.Count - 1);
            indices.Add(vertices.Count - 4);

        }

        public enum Type {
            Box, Slant, Cyllinder, BlockStair, SlantStair, SeparateStair, Freeform
        }
    }
}
