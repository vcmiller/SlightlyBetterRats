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
    [CustomEditor(typeof(SplineMesh))]
    public class SplineMeshInspector : UnityEditor.Editor {
        private SplineMesh myTarget { get { return target as SplineMesh; } }

        public override void OnInspectorGUI() {
            if (!myTarget.enabled) {
                GUILayout.Label("Controls inactive while disabled.");
                return;
            }
            EditorGUI.BeginDisabledGroup(!myTarget.profile);
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Export Mesh")) {
                ExportMesh(myTarget.ownedMesh, myTarget.name);
            }
            
            EditorGUI.BeginDisabledGroup(!myTarget.ownedCollision || !myTarget.profile || !myTarget.profile.separateCollisionMesh);
            if (GUILayout.Button("Export Collision")) {
                ExportMesh(myTarget.ownedCollision, myTarget.name + "_Collision");
            }
            EditorGUI.EndDisabledGroup();

            GUILayout.EndHorizontal();

            if (GUILayout.Button("Export Meshes and Convert")) {
                Mesh mesh = ExportMesh(myTarget.ownedMesh, myTarget.name);
                Mesh colMesh = null;
                if (mesh && myTarget.ownedCollision && myTarget.profile.separateCollisionMesh) {
                    colMesh = ExportMesh(myTarget.ownedCollision, myTarget.name + "_Collision");
                }

                if (mesh) {
                    Undo.RecordObject(myTarget, "Export Spline Meshes");
                    foreach (var mf in myTarget.filters) {
                        mf.sharedMesh = mesh;
                    }
                    foreach (var mc in myTarget.colliders) {
                        mc.sharedMesh = colMesh != null ? colMesh : mesh;
                    }

                    myTarget.enabled = false;
                }
            }

            EditorGUI.EndDisabledGroup();

            if (myTarget) {
                serializedObject.Update();
                DrawPropertiesExcluding(serializedObject, "m_Script");
                serializedObject.ApplyModifiedProperties();
            }
        }

        private Mesh ExportMesh(Mesh mesh, string name) {
            var path = EditorUtility.SaveFilePanelInProject("Save New Mesh", name + ".asset", "asset", "Save new asset to file");
            if (path.Length > 0) {
                var result = Instantiate(mesh);
                Unwrapping.GenerateSecondaryUVSet(result);
                AssetDatabase.CreateAsset(result, path);
                AssetDatabase.Refresh();
                return result;
            } else {
                return null;
            }
        }
    }
}