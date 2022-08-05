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

using UnityEditor;
using UnityEngine;

namespace SBR.Editor {
    public static class SplineCreator {
        [MenuItem("GameObject/3D Object/Spline/Empty")]
        public static Spline CreateEmptySpline() {
            GameObject splineObj = new GameObject("Spline");
            splineObj.isStatic = true;

            Spline spline = splineObj.AddComponent<Spline>();

            Selection.activeGameObject = splineObj;
            return spline;
        }

        [MenuItem("GameObject/3D Object/Spline/Line")]
        public static Spline CreateLineSpline() {
            var spline = CreateEmptySpline();

            spline.spline.points = new SplineData.Point[] {
                new SplineData.Point() { position = new Vector3(0, 0, 0), tangent = new Vector3(0, 0, 2) },
                new SplineData.Point() { position = new Vector3(0, 0, 10), tangent = new Vector3(0, 0, 2) }
            };

            return spline;
        }

        [MenuItem("GameObject/3D Object/Spline/Circle")]
        public static Spline CreateCircleSpline() {
            var spline = CreateEmptySpline();

            spline.spline.points = new SplineData.Point[] {
                new SplineData.Point() { position = new Vector3(10, 0, 0), tangent = new Vector3(0, 0, 5.5f) },
                new SplineData.Point() { position = new Vector3(0, 0, 10), tangent = new Vector3(-5.5f, 0, 0) },
                new SplineData.Point() { position = new Vector3(-10, 0, 0), tangent = new Vector3(0, 0, -5.5f) },
                new SplineData.Point() { position = new Vector3(0, 0, -10), tangent = new Vector3(5.5f, 0, 0) },
            };
            spline.spline.closed = true;

            return spline;
        }

        [MenuItem("GameObject/3D Object/Spline/Mesh")]
        public static SplineMesh CreateSplineMesh() {
            var spline = CreateLineSpline();
            var mr = spline.gameObject.AddComponent<MeshRenderer>();
            spline.gameObject.AddComponent<MeshFilter>();
            spline.gameObject.AddComponent<MeshCollider>();

            mr.sharedMaterial = BrushCreator.defaultMat;

            var mesh = spline.gameObject.AddComponent<SplineMesh>();
            mesh.spline = spline;
            return mesh;
        }
    }
}
