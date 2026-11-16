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
    [Serializable]
    public class Vertex {
        [FormerlySerializedAs("position")]
        public Vector3 _position;

        public Vertex(Vector3 position) {
            this._position = position;
        }

        public Vertex() { }
    }

    [Serializable]
    public class Face {
        [FormerlySerializedAs("vertices")]
        public List<int> _vertices;

        public Face(IEnumerable<int> vertices) {
            this._vertices = new List<int>(vertices);
        }

        public Face(params int[] vertices) : this((IEnumerable<int>)vertices) { }

        public Vector3 GetNormal(FreeformMesh mesh) {
            if (_vertices == null || _vertices.Count < 3) {
                return Vector3.zero;
            }

            Vector3 normal = Vector3.zero;

            for (int i = 0; i < _vertices.Count; i++) {
                int j = (i + 1) % _vertices.Count;

                Vector3 v1 = mesh._vertices[_vertices[i]]._position;
                Vector3 v2 = mesh._vertices[_vertices[j]]._position;

                normal.x += (v1.y - v2.y) * (v1.z + v2.z);
                normal.y += (v1.z - v2.z) * (v1.x + v2.x);
                normal.z += (v1.x - v2.x) * (v1.y + v2.y);
            }

            return normal.normalized;
        }

        public Vector3[] GetPointsArray(Transform transform, FreeformMesh mesh) {
            List<Vector3> points = new List<Vector3>();

            foreach (int v in _vertices) {
                Vector3 pos = mesh._vertices[v]._position;
                pos = transform.TransformPoint(pos);
                points.Add(pos);
            }

            return points.ToArray();
        }

        public Vector3 GetCenter(FreeformMesh mesh) {
            return mesh.GetVerticesCenter(_vertices);
        }

        public override bool Equals(object obj) {
            Face f = obj as Face;

            if (f == null) {
                return false;
            }

            int s = f._vertices.IndexOf(_vertices[0]);

            if (_vertices.Count != f._vertices.Count || s < 0) {
                return false;
            }

            bool fwd = true, rev = true;
            for (int i = 0; i < _vertices.Count; i++) {
                fwd &= _vertices[i] == f._vertices[(s + i) % f._vertices.Count];
                rev &= _vertices[i] == f._vertices[(s - i + f._vertices.Count) % f._vertices.Count];
            }

            return fwd || rev;
        }

        public override int GetHashCode() {
            int code = 0;

            foreach (int i in _vertices) {
                code ^= i.GetHashCode();
            }

            return code;
        }

        public Face() { }
    }

    [Serializable]
    public class FreeformMesh {
        [FormerlySerializedAs("faces")]
        public List<Face> _faces;
        [FormerlySerializedAs("vertices")]
        public List<Vertex> _vertices;

        [NonSerialized]
        private Dictionary<UnorderedPair<int>, List<int>> _edgeFaces;
        [NonSerialized]
        private Dictionary<int, HashSet<int>> _vertexNeighbors;
        [NonSerialized]
        private Dictionary<int, HashSet<int>> _vertexFaces;
        [NonSerialized]
        private Dictionary<int, HashSet<UnorderedPair<int>>> _faceEdges;

        [NonSerialized]
        private bool _graphDirty = true;

        public FreeformMesh() {
            _faces = new List<Face>();
            _vertices = new List<Vertex>();
        }

        public void ForceGraphUpdate() {
            _graphDirty = true;
        }

        private void UpdateGraph() {
            if (!_graphDirty) return;
            _edgeFaces = new Dictionary<UnorderedPair<int>, List<int>>();
            _vertexNeighbors = new Dictionary<int, HashSet<int>>();
            _vertexFaces = new Dictionary<int, HashSet<int>>();
            _faceEdges = new Dictionary<int, HashSet<UnorderedPair<int>>>();

            for (int i = 0; i < _faces.Count; i++) {
                Face face = _faces[i];

                for (int j = 0; j < face._vertices.Count; j++) {
                    int next = face._vertices[(j + 1) % face._vertices.Count];
                    int prev = face._vertices[((j - 1) + face._vertices.Count) % face._vertices.Count];

                    int vert = face._vertices[j];
                    if (!_vertexFaces.ContainsKey(vert)) {
                        _vertexFaces[vert] = new HashSet<int>();
                    }

                    _vertexFaces[vert].Add(i);

                    if (!_vertexNeighbors.ContainsKey(vert)) {
                        _vertexNeighbors[vert] = new HashSet<int>();
                    }

                    _vertexNeighbors[vert].Add(prev);
                    _vertexNeighbors[vert].Add(next);

                    UnorderedPair<int> edge1 = new UnorderedPair<int>(prev, vert);

                    if (!_edgeFaces.ContainsKey(edge1)) {
                        _edgeFaces[edge1] = new List<int>();
                    }

                    _edgeFaces[edge1].Add(i);
                }
            }

            foreach (KeyValuePair<UnorderedPair<int>, List<int>> pair in _edgeFaces) {
                foreach (int face in pair.Value) {
                    if (!_faceEdges.ContainsKey(face)) {
                        _faceEdges[face] = new HashSet<UnorderedPair<int>>();
                    }

                    _faceEdges[face].Add(pair.Key);
                }
            }

            _graphDirty = false;
        }

        private bool EdgesAreContiguous(ICollection<UnorderedPair<int>> edges, out List<int> vertices) {
            UpdateGraph();
            IEnumerator<UnorderedPair<int>> iter = edges.GetEnumerator();
            iter.MoveNext();
            UnorderedPair<int> edge = iter.Current;
            iter.Dispose();

            int t1, t2;
            if (!GetEdgeOrder(edge, out t1, out t2)) {
                vertices = null;
                return false;
            }

            vertices = new List<int> {t1, t2};

            for (int i = 1; i < edges.Count; i++) {
                int prev = vertices[vertices.Count - 2];
                int cur = vertices[vertices.Count - 1];

                UnorderedPair<int> curEdge = new UnorderedPair<int>(prev, cur);

                if (_edgeFaces[curEdge].Count > 1) {
                    vertices = null;
                    return false;
                }

                bool found = false;
                foreach (UnorderedPair<int> e in edges) {
                    if (e == curEdge || (e.Item1 != cur && e.Item2 != cur)) continue;
                    int next = e.Not(cur);

                    if (next == vertices[0]) {
                        if (i == edges.Count - 1) {
                            return true;
                        } else {
                            vertices = null;
                            return false;
                        }
                    }

                    vertices.Add(next);
                    found = true;
                    break;
                }

                if (found) continue;
                vertices = null;
                return false;
            }

            vertices = null;
            return false;
        }

        public bool FacesAreConnected(IEnumerable<int> faces) {
            UpdateGraph();

            HashSet<int> faceSet = new HashSet<int>(faces);
            Queue<int> frontier = new Queue<int>();

            IEnumerator<int> en = faces.GetEnumerator();
            en.MoveNext();
            int first = en.Current;
            en.Dispose();

            frontier.Enqueue(first);
            faceSet.Remove(first);

            while (faceSet.Count > 0) {
                if (frontier.Count == 0) {
                    return false;
                }

                int front = frontier.Dequeue();
                HashSet<UnorderedPair<int>> edges = _faceEdges[front];

                foreach (UnorderedPair<int> edge in edges) {
                    foreach (int f in _edgeFaces[edge]) {
                        if (faceSet.Remove(f)) {
                            frontier.Enqueue(f);
                        }
                    }
                }
            }

            return true;
        }

        public void Clear() {
            _vertices.Clear();
            _faces.Clear();
        }

        public int AddVertex(Vector3 vertex) {
            if (_vertices == null) {
                _vertices = new List<Vertex>();
            }

            _graphDirty = true;

            _vertices.Add(new Vertex(vertex));
            return _vertices.Count - 1;
        }

        public bool AddFace(params int[] verts) {
            return AddFace((IEnumerable<int>) verts);
        }

        public bool AddFace(IEnumerable<int> verts) {
            if (_faces == null) {
                _faces = new List<Face>();
            }

            Face newFace = new Face(verts);

            if (!_faces.Contains(newFace)) {
                _faces.Add(new Face(verts));
                _graphDirty = true;
                return true;
            } else {
                return false;
            }
        }

        private bool GetEdgeOrder(UnorderedPair<int> edge, out int t1, out int t2) {
            UpdateGraph();

            List<int> n = _edgeFaces[edge];

            if (n.Count != 1) {
                t1 = -1;
                t2 = -1;
                return false;
            }

            Face f = _faces[n[0]];

            int ind1 = f._vertices.IndexOf(edge.Item1);
            int ind2 = f._vertices.IndexOf(edge.Item2);

            if (ind2 == (ind1 + 1) % f._vertices.Count) {
                t1 = edge.Item2;
                t2 = edge.Item1;
                return true;
            } else if (ind1 == (ind2 + 1) % f._vertices.Count) {
                t1 = edge.Item1;
                t2 = edge.Item2;
                return true;
            } else {
                t1 = -1;
                t2 = -1;
                return false;
            }
        }

        public bool AddFace(ICollection<UnorderedPair<int>> edges) {
            List<int> vertLoop;

            if (edges.Count > 2 && EdgesAreContiguous(edges, out vertLoop)) {
                return AddFace(vertLoop);
            } else if (edges.Count == 2) {
                IEnumerator<UnorderedPair<int>> iter = edges.GetEnumerator();
                iter.MoveNext();
                UnorderedPair<int> e1 = iter.Current;
                iter.MoveNext();
                UnorderedPair<int> e2 = iter.Current;
                iter.Dispose();

                int v1, v2, v3, v4;
                if (GetEdgeOrder(e1, out v1, out v2) && GetEdgeOrder(e2, out v3, out v4)) {
                    if (v1 == v4) {
                        return AddFace(v1, v2, v3);
                    } else if (v2 == v3) {
                        return AddFace(v1, v2, v4);
                    } else {
                        return AddFace(v1, v2, v3, v4);
                    }
                }
            }

            return false;
        }

        public Vector3 GetVerticesCenter(ICollection<int> verts) {
            Vector3 c = verts.Aggregate(Vector3.zero, (current, index) => current + _vertices[index]._position);

            return c / verts.Count;
        }

        public Vector3 GetEdgeCenter(UnorderedPair<int> edge) {
            return (_vertices[edge.Item1]._position + _vertices[edge.Item2]._position) / 2.0f;
        }

        public Vector3 GetFacesNormal(ICollection<int> faces) {
            Vector3 n = faces.Aggregate(Vector3.zero, (current, index) => current + this._faces[index].GetNormal(this));


            if (n.sqrMagnitude < 0.01f) {
                IEnumerator<int> iter = faces.GetEnumerator();
                iter.MoveNext();
                int c = iter.Current;
                iter.Dispose();

                return this._faces[c].GetNormal(this);
            } else {
                return n.normalized;
            }

        }

        public HashSet<int> GetUniqueVertexList(IEnumerable<UnorderedPair<int>> edges) {
            HashSet<int> result = new HashSet<int>();

            foreach (UnorderedPair<int> edge in edges) {
                result.Add(edge.Item1);
                result.Add(edge.Item2);
            }

            return result;
        }

        public HashSet<int> GetUniqueVertexList(IEnumerable<int> faces) {
            HashSet<int> result = new HashSet<int>();

            foreach (int face in faces) {
                foreach (int vertex in this._faces[face]._vertices) {
                    result.Add(vertex);
                }
            }

            return result;
        }

        public List<UnorderedPair<int>> GetEdges() {
            List<UnorderedPair<int>> edges = new List<UnorderedPair<int>>();

            foreach (Face f in _faces) {
                for (int i = 0; i < f._vertices.Count; i++) {
                    int j = (i + 1) % f._vertices.Count;

                    UnorderedPair<int> edge = new UnorderedPair<int>(f._vertices[i], f._vertices[j]);

                    if (!edges.Contains(edge)) {
                        edges.Add(edge);
                    }
                }
            }

            return edges;
        }

        private bool IsNotInnerEdge(UnorderedPair<int> edge, ICollection<int> faceIndices) {
            foreach (int neighbor in _edgeFaces[edge]) {
                if (!faceIndices.Contains(neighbor)) {
                    return true;
                }
            }

            return false;
        }

        private bool IsOuterEdge(UnorderedPair<int> edge, ICollection<int> faceIndices) {
            bool outer = false, inner = false;

            foreach (int neighbor in _edgeFaces[edge]) {
                if (!faceIndices.Contains(neighbor)) {
                    outer = true;
                } else {
                    inner = true;
                }

                if (outer && inner) {
                    return true;
                }
            }

            return false;
        }

        private UnorderedPair<int> FindOuterEdge(ICollection<int> faceIndices) {

            foreach (int face in faceIndices) {
                foreach (UnorderedPair<int> edge in _faceEdges[face]) {
                    if (IsNotInnerEdge(edge, faceIndices)) {
                        return edge;
                    }
                }
            }

            return new UnorderedPair<int>(-1, -1);
        }

        public void DeleteFaces(IEnumerable<int> f) {
            HashSet<Face> toRemove = new HashSet<Face>();
            HashSet<int> affectedVerts = new HashSet<int>();

            foreach (int face in f) {
                toRemove.Add(_faces[face]);

                foreach (int vert in _faces[face]._vertices) {
                    affectedVerts.Add(vert);
                }
            }

            _faces.RemoveAll(face => toRemove.Contains(face));

            _graphDirty = true;


            DeleteVertices(affectedVerts, v => !_vertexNeighbors.ContainsKey(v) || _vertexNeighbors[v].Count == 0);
        }

        public void DeleteEdges(IEnumerable<UnorderedPair<int>> edges) {
            UpdateGraph();
            HashSet<int> affectedVertices = GetUniqueVertexList(edges);

            foreach (UnorderedPair<int> edge in edges) {
                List<int> neighbors = _edgeFaces[edge];

                // We only delete edges that have exactly two neighboring faces, because we need to merge those faces.
                if (neighbors.Count != 2) continue;
                int f1 = neighbors[0];
                int f2 = neighbors[1];

                int delFace = f1;
                int keepFace = f2;

                int start = _faces[delFace]._vertices.IndexOf(edge.Item1);
                int next = (start + 1) % _faces[delFace]._vertices.Count;

                if (_faces[delFace]._vertices[next] == edge.Item2) {
                    delFace = f2;
                    keepFace = f1;

                    start = _faces[delFace]._vertices.IndexOf(edge.Item1);
                }

                List<int> transfer = new List<int>();

                List<int> delVerts = _faces[delFace]._vertices;
                List<int> keepVerts = _faces[keepFace]._vertices;

                for (int i = (start + 1) % delVerts.Count; delVerts[i] != edge.Item2; i = (i + 1) % delVerts.Count) {
                    transfer.Add(delVerts[i]);
                }

                int pos = keepVerts.IndexOf(edge.Item1);
                keepVerts.InsertRange(pos + 1, transfer);

                // Mark face for deletion. This is easier than remembering the indices of marked faces.
                _faces[delFace]._vertices = null;

                // Do some cleanup. This allows us to delete a list of edges without completely rebuilding the graph each time.
                _edgeFaces.Remove(edge);

                foreach (KeyValuePair<UnorderedPair<int>, List<int>> ef in _edgeFaces) {
                    int i = ef.Value.IndexOf(delFace);
                    if (i >= 0) {
                        ef.Value[i] = keepFace;
                    }
                }
            }

            _faces.RemoveAll(face => face._vertices == null);

            _graphDirty = true;
            DeleteVertices(affectedVertices, v => !_vertexNeighbors.ContainsKey(v) || _vertexNeighbors[v].Count == 2);
        }

        private void DeleteVertices(IEnumerable<int> affectedVertices, Predicate<int> pred) {
            UpdateGraph();

            List<int> deleteVerts = new List<int>();

            foreach (int vert in affectedVertices) {

                if (pred(vert)) {
                    deleteVerts.Add(vert);
                    if (_vertexFaces.ContainsKey(vert)) {
                        HashSet<int> vf = _vertexFaces[vert];

                        foreach (int face in vf) {
                            _faces[face]._vertices.Remove(vert);
                        }
                    }
                }
            }

            // Delete vertices that are no longer needed.
            for (int i = 0; i < deleteVerts.Count; i++) {
                _vertices.RemoveAt(deleteVerts[i]);

                foreach (Face face in _faces) {
                    for (int j = 0; j < face._vertices.Count; j++) {
                        if (face._vertices[j] > deleteVerts[i]) {
                            face._vertices[j]--;
                        }
                    }
                }

                for (int j = i + 1; j < deleteVerts.Count; j++) {
                    if (deleteVerts[j] > deleteVerts[i]) {
                        deleteVerts[j]--;
                    }
                }
            }

            _graphDirty = true;
        }

        public int WeldVertices(ICollection<int> selectedVertices) {
            int keep = AddVertex(GetVerticesCenter(selectedVertices));

            foreach (Face face in _faces) {
                for (int i = 0; i < face._vertices.Count; i++) {
                    if (selectedVertices.Contains(face._vertices[i])) {
                        face._vertices[i] = keep;
                    }
                }
            }

            DeleteVertices(selectedVertices, vert => true);
            _graphDirty = true;

            return _vertices.Count - 1;
        }

        public bool InsertLoop(ICollection<UnorderedPair<int>> edges) {
            if (edges.Count != 1) {
                return false;
            }

            UpdateGraph();

            IEnumerator<UnorderedPair<int>> iter = edges.GetEnumerator();
            iter.MoveNext();
            UnorderedPair<int> edge = iter.Current;
            iter.Dispose();

            List<UnorderedPair<int>> edgeLoop = new List<UnorderedPair<int>> {edge};
            List<int> faceLoop = new List<int> {_edgeFaces[edge][0]};

            bool oneSide = false;

            while (true) {
                int cnt = faceLoop.Count;
                int cur = faceLoop[cnt - 1];

                foreach (UnorderedPair<int> e in _faceEdges[cur]) {
                    if (!e.Adjacent(edgeLoop[edgeLoop.Count - 1])) {
                        edgeLoop.Add(e);

                        if (e == edgeLoop[0]) {
                            break;
                        }

                        List<int> ef = _edgeFaces[e];

                        foreach (int f in ef) {
                            if (f != cur && _faceEdges[f].Count == 4 && f != faceLoop[0]) {
                                faceLoop.Add(f);
                                break;
                            }
                        }

                        break;
                    }
                }

                if (faceLoop.Count == cnt || edgeLoop[edgeLoop.Count - 1] == edgeLoop[0]) {
                    oneSide = edgeLoop[edgeLoop.Count - 1] != edgeLoop[0];
                    break;
                }
            }

            if (oneSide) {
                while (true) {
                    int cnt = faceLoop.Count;
                    UnorderedPair<int> cur = edgeLoop[0];

                    foreach (int f in _edgeFaces[cur]) {
                        if (f != faceLoop[0] && _faceEdges[f].Count == 4) {
                            faceLoop.Insert(0, f);

                            foreach (UnorderedPair<int> e in _faceEdges[f]) {
                                if (!e.Adjacent(cur)) {
                                    edgeLoop.Insert(0, e);
                                    break;
                                }
                            }

                            break;
                        }
                    }

                    if (faceLoop.Count == cnt) {
                        break;
                    }
                }
            }

            List<int> addedVerts = new List<int>();

            for (int i = 0; i < faceLoop.Count; i++) {
                Face face = _faces[faceLoop[i]];
                UnorderedPair<int> edge1 = edgeLoop[i];
                UnorderedPair<int> edge2 = edgeLoop[i + 1];

                int v1, v2;

                if (i == 0) {
                    v1 = AddVertex(GetEdgeCenter(edge1));
                    addedVerts.Add(v1);
                } else {
                    v1 = addedVerts[addedVerts.Count - 1];
                }

                if (i == faceLoop.Count - 1 && edge2 == edgeLoop[0]) {
                    v2 = addedVerts[0];
                } else {
                    v2 = AddVertex(GetEdgeCenter(edge2));
                    addedVerts.Add(v2);
                }

                Face newFace = new Face();
                newFace._vertices = new List<int>();

                InsertVertex(face, edge1, v1);
                InsertVertex(face, edge2, v2);

                int ind1 = face._vertices.IndexOf(v1);
                int ind2 = face._vertices.IndexOf(v2);

                if (ind1 == -1 || ind2 == -1) {
                    Debug.LogError("Could not insert vertex");
                    continue;
                }

                for (int j = ind1; j != (ind2 + 1) % face._vertices.Count; j = (j + 1) % face._vertices.Count) {
                    newFace._vertices.Add(face._vertices[j]);

                    if (j != ind1 && j != ind2) {
                        face._vertices[j] = -1;
                    }
                }

                face._vertices.RemoveAll(v => v < 0);
                _faces.Add(newFace);
            }

            _graphDirty = true;
            return true;
        }

        private void InsertVertex(Face face, UnorderedPair<int> edge, int vertex) {
            int i1 = face._vertices.IndexOf(edge.Item1);
            int i2 = face._vertices.IndexOf(edge.Item2);

            if (i2 == (i1 + 1) % face._vertices.Count) {
                face._vertices.Insert(i1 + 1, vertex);
                _graphDirty = true;
            } else if (i1 == (i2 + 1) % face._vertices.Count) {
                face._vertices.Insert(i2 + 1, vertex);
                _graphDirty = true;
            } else {
                Debug.Log("Vertex not inserted!");
            }
        }

        public bool ExtrudeFaces(HashSet<int> selected) {
            if (!FacesAreConnected(selected)) {
                Debug.LogError("Cannot Extrude Disconnected Faces");
                return false;
            }

            UnorderedPair<int> startEdge = FindOuterEdge(selected);

            if (startEdge.Item1 == -1) {
                Debug.LogError("Cannot Find Outer Edge");
                return false;
            }

            // Make sure the first two vertices are in the right order by comparing with a selected face
            foreach (int f in _edgeFaces[startEdge]) {
                if (!selected.Contains(f)) continue;
                int t1Index = _faces[f]._vertices.IndexOf(startEdge.Item1);
                int t2Index = _faces[f]._vertices.IndexOf(startEdge.Item2);

                if (t1Index == (t2Index + 1) % _faces[f]._vertices.Count) {
                    // Edge is in the wrong order. Fix it (this doesn't modify the orignal edge).
                    int temp = startEdge.Item2;
                    startEdge = new UnorderedPair<int>(temp, startEdge.Item1);
                }

                break;
            }

            Vector3 normal = GetFacesNormal(selected);
            List<int> edgeLoop = new List<int> {startEdge.Item1, startEdge.Item2};

            while (true) {
                int cur = edgeLoop[edgeLoop.Count - 1];
                int prev = edgeLoop[edgeLoop.Count - 2];
                int next = -1;

                foreach (int cand in _vertexNeighbors[cur]) {
                    UnorderedPair<int> edge = new UnorderedPair<int>(cur, cand);

                    if (cand == prev || !IsOuterEdge(edge, selected)) continue;
                    next = cand;
                    break;
                }

                if (next != -1) {
                    if (next == edgeLoop[0]) {
                        break;
                    }

                    edgeLoop.Add(next);
                } else {
                    Debug.LogError("Edge Loop Not Found");
                    return false;
                }
            }

            Face lastCreatedFace = null;

            int v1 = -1, v2 = -1;

            foreach (int v in edgeLoop) {
                int newVert = AddVertex(_vertices[v]._position);

                if (lastCreatedFace != null) {
                    lastCreatedFace._vertices.Add(v);
                    lastCreatedFace._vertices.Add(newVert);
                } else {
                    v1 = v;
                    v2 = newVert;
                }

                lastCreatedFace = new Face(newVert, v);
                _faces.Add(lastCreatedFace);

                foreach (int face in selected) {
                    for (int k = 0; k < _faces[face]._vertices.Count; k++) {
                        if (_faces[face]._vertices[k] == v) {
                            _faces[face]._vertices[k] = newVert;
                        }
                    }
                }
            }

            lastCreatedFace._vertices.Add(v1);
            lastCreatedFace._vertices.Add(v2);

            foreach (int v in GetUniqueVertexList(selected)) {
                _vertices[v]._position += normal;
            }

            _graphDirty = true;

            return true;
        }

        public void SelectEdgeLoop(ICollection<UnorderedPair<int>> edges, UnorderedPair<int> start, bool recurses = true) {
            UpdateGraph();
            edges.Add(start);

            int prev = start.Item1;
            int cur = start.Item2;
            while (true) {
                UnorderedPair<int> curEdge = new UnorderedPair<int>(prev, cur);

                if (_vertexNeighbors[cur].Count != 4) {
                    break;
                }

                bool found = false;
                foreach (int v in _vertexNeighbors[cur]) {
                    UnorderedPair<int> e = new UnorderedPair<int>(cur, v);

                    bool ok = true;
                    foreach (int f in _edgeFaces[e]) {
                        if (_edgeFaces[curEdge].Contains(f)) {
                            ok = false;
                            break;
                        }
                    }

                    if (ok) {
                        found = true;
                        edges.Add(e);
                        prev = cur;
                        cur = e.Not(cur);
                        break;
                    }
                }

                if (!found || cur == start.Item1) {
                    break;
                }
            }

            if (recurses && cur != start.Item1) {
                SelectEdgeLoop(edges, new UnorderedPair<int>(start.Item2, start.Item1), false);
            }
        }

        public void Select(Transform transform, Vector2 mousePos, Camera camera, out int selVertex, out int selFace, out UnorderedPair<int> selEdge, float radius = 10, float bump = 0.2f) {
            selVertex = -1;
            selFace = -1;
            selEdge = new UnorderedPair<int>(-1, -1);

            float minDist = Mathf.Infinity;

            Ray ray = camera.ScreenPointToRay(mousePos);

            ray.origin = transform.InverseTransformPoint(ray.origin);
            ray.direction = transform.InverseTransformDirection(ray.direction);

            for (int v = 0; v < _vertices.Count; v++) {
                Vertex vert = _vertices[v];

                Vector3 world = transform.TransformPoint(vert._position);
                Vector3 screen = camera.WorldToScreenPoint(world);
                if (screen.z > 0) {
                    screen.z = 0;
                    float mouseDist2 = Vector2.SqrMagnitude((Vector2)screen - mousePos);

                    float camDist = Vector3.Distance(camera.transform.position, world) - bump * 3;
                    if (mouseDist2 < radius * radius && camDist < minDist) {
                        selVertex = v;
                        minDist = camDist;
                    }
                }
            }

            foreach (UnorderedPair<int> e in GetEdges()) {
                Vertex vert1 = _vertices[e.Item1];
                Vertex vert2 = _vertices[e.Item2];

                Vector3 v1 = vert1._position;
                Vector3 v2 = vert2._position;
                Vector3 segment = v2 - v1;

                Vector3 c = Vector3.Cross(camera.transform.forward, segment);
                Vector3 n = Vector3.Cross(c, segment);
                Plane plane = new Plane(n, v1);
                float camDist;

                if (plane.Raycast(ray, out camDist) && camDist - bump < minDist) {
                    Vector3 hitPos = ray.GetPoint(camDist);
                    Vector3 hitProj = Math3D.ProjectPointOnLineSegment(v1, v2, hitPos);

                    Vector3 projScreen = camera.WorldToScreenPoint(transform.TransformPoint(hitProj));
                    if (projScreen.z > 0 && Vector2.Distance(projScreen, mousePos) < radius * 0.6f) {
                        selEdge = e;
                        selVertex = -1;
                        minDist = camDist - bump;
                    }
                }
            }

            for (int f = 0; f < _faces.Count; f++) {
                Face face = _faces[f];
                Vector3 normal = face.GetNormal(this);
                Plane plane = new Plane(normal, _vertices[face._vertices[0]]._position);

                float dist;
                if (plane.Raycast(ray, out dist) && dist < minDist) {
                    Vector3 hitPos = ray.GetPoint(dist);

                    bool inside = true;

                    for (int i = 0; i < face._vertices.Count; i++) {

                        int j = (i + 1) % face._vertices.Count;

                        Vector3 v1 = _vertices[face._vertices[i]]._position;
                        Vector3 v2 = _vertices[face._vertices[j]]._position;

                        Vector3 edge = v2 - v1;
                        Vector3 toHit = hitPos - v1;
                        Vector3 cross = Vector3.Cross(edge, toHit);

                        if (Vector3.Dot(cross, normal) < 0) {
                            inside = false;
                            break;
                        }
                    }

                    if (inside) {
                        selVertex = -1;
                        selEdge = new UnorderedPair<int>(-1, -1);
                        selFace = f;
                        minDist = dist;
                    }
                }
            }
        }

        public void GetMeshData(List<Vector3> vertexList, List<Vector2> uvList, List<int> indexList, Vector3 textureWorldScale, Vector2 textureLocalScale) {
            foreach (Face face in _faces) {
                Vector3 normal = face.GetNormal(this);

                float nx = Mathf.Abs(normal.x);
                float ny = Mathf.Abs(normal.y);
                float nz = Mathf.Abs(normal.z);

                int projMode;
                Vector2 mult = new Vector2(1, 1);

                if (nx >= ny && nx >= nz) {
                    projMode = 0;
                    mult.x = Mathf.Sign(normal.x);
                } else if (ny >= nx && ny >= nz) {
                    projMode = 1;
                    mult.y = Mathf.Sign(normal.y);
                } else {
                    projMode = 2;
                    mult.x = -Mathf.Sign(normal.z);
                }

                mult.Scale(textureLocalScale);

                int startIndex = vertexList.Count;

                foreach (int index in face._vertices) {
                    Vector3 vertex = _vertices[index]._position;

                    vertexList.Add(vertex);

                    Vector3 vMul = vertex;
                    vMul.Scale(textureWorldScale);

                    if (projMode == 0) {
                        uvList.Add(new Vector2(vMul.z * mult.x, vMul.y * mult.y));
                    } else if (projMode == 1) {
                        uvList.Add(new Vector2(vMul.x * mult.x, vMul.z * mult.y));
                    } else {
                        uvList.Add(new Vector2(vMul.x * mult.x, vMul.y * mult.y));
                    }
                }

                for (int i = 1; i < face._vertices.Count - 1; i++) {
                    indexList.Add(startIndex);
                    indexList.Add(startIndex + i);
                    indexList.Add(startIndex + i + 1);
                }
            }
        }
    }
}
