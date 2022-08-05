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

using Infohazard.Core;
using UnityEngine;
using UnityEditor;

namespace SBR.Editor {
    public static class BrushCreator {
        private static Material _defaultMat;
        public static Material defaultMat {
            get {
                if (!_defaultMat) {
                    MeshRenderer mr = GameObject.CreatePrimitive(PrimitiveType.Cube).GetComponent<MeshRenderer>();
                    _defaultMat = mr.sharedMaterial;
                    Object.DestroyImmediate(mr.gameObject);
                }

                return _defaultMat;
            }
        }

        [MenuItem("GameObject/3D Object/Brush/Box")]
        public static void CreateBoxBrush() {
            CreateBrush(Brush.Type.Box);
        }

        [MenuItem("GameObject/3D Object/Brush/Slant")]
        public static void CreateSlantBrush() {
            CreateBrush(Brush.Type.Slant);
        }

        [MenuItem("GameObject/3D Object/Brush/Cyllinder")]
        public static void CreateCyllinderBrush() {
            CreateBrush(Brush.Type.Cyllinder);
        }

        [MenuItem("GameObject/3D Object/Brush/Block Stair")]
        public static void CreateBlockStairBrush() {
            CreateBrush(Brush.Type.BlockStair);
        }

        [MenuItem("GameObject/3D Object/Brush/Slant Stair")]
        public static void CreateSlantStairBrush() {
            CreateBrush(Brush.Type.SlantStair);
        }

        [MenuItem("GameObject/3D Object/Brush/Separate Stair")]
        public static void CreateSeparateStairBrush() {
            CreateBrush(Brush.Type.SeparateStair);
        }

        [MenuItem("GameObject/3D Object/Brush/Freeform")]
        public static void CreateFreeformBrush() {
            CreateBrush(Brush.Type.Freeform);
        }

        private static void CreateBrush(Brush.Type type) {
            GameObject brushObj = new GameObject(type + " Brush");
            brushObj.isStatic = true;

            if (Selection.activeGameObject) {
                brushObj.transform.SetParentAndReset(Selection.activeGameObject.transform);
            }

            Brush brush = brushObj.AddComponent<Brush>();
            brush.type = type;
            brush.GetComponent<MeshRenderer>().sharedMaterial = defaultMat;

            Selection.activeGameObject = brushObj;
        }
    }
    
}
