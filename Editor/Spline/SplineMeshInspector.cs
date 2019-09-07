using System.Collections;
using System.Collections.Generic;
using System.Linq;
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