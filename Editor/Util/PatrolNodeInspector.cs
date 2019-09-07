using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SBR.Editor {
    [CustomEditor(typeof(PatrolNode))]
    public class PatrolNodeInspector : UnityEditor.Editor {
        private static bool inspectingNode;
        private static bool animate = true;

        private PatrolNode nodeTarget => target as PatrolNode;

        [MenuItem("GameObject/3D Object/Patrol Node")]
        public static void CreatePatrolNode() {
            GameObject node = new GameObject("Patrol Node");
            node.AddComponent<PatrolNode>();
            Selection.activeGameObject = node;
        }

        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            animate = EditorGUILayout.Toggle("Animate", animate);
        }

        private void OnSceneGUI() {
            var node = nodeTarget;
            Vector3 pos = node.transform.position;
            float size = HandleUtility.GetHandleSize(pos) * 0.2f;
            pos -= Camera.current.transform.up * size * 2;
            Handles.color = Color.white;
            if (Handles.Button(pos, Quaternion.identity, size, size, Handles.SphereHandleCap)) {
                var newNode = Instantiate(node);
                newNode.name = node.name;
                newNode.transform.position += node.transform.forward;
                node.next = newNode;
                Selection.activeGameObject = newNode.gameObject;
            }
        }

        private void OnEnable() {
            inspectingNode = true;
            EditorApplication.update += EditorApplicationUpdate;
        }

        private void OnDisable() {
            inspectingNode = false;
            EditorApplication.update -= EditorApplicationUpdate;
        }

        private void EditorApplicationUpdate() {
            SceneView.RepaintAll();
        }

        [DrawGizmo(GizmoType.Active | GizmoType.NotInSelectionHierarchy
             | GizmoType.InSelectionHierarchy | GizmoType.Pickable, typeof(PatrolNode))]
        internal static void DrawPathGizmos(PatrolNode node, GizmoType selectionType) {
            if (node.next) {
                Vector3 a = node.transform.position;
                Vector3 b = node.next.transform.position;

                Color c = inspectingNode ? Color.white : new Color(1, 1, 1, 0.4f);

                Color colorOld = Gizmos.color;
                Gizmos.color = c;
                Gizmos.DrawLine(a, b);
                Gizmos.color = colorOld;

                if (inspectingNode && animate) {
                    colorOld = Handles.color;
                    Handles.color = Color.white;
                    var p = Vector3.Lerp(a, b, Time.realtimeSinceStartup % 1);
                    float s = HandleUtility.GetHandleSize(p) * 0.03f;
                    Handles.DotHandleCap(0, p, Quaternion.identity, s, EventType.Repaint);
                    Handles.color = colorOld;
                }
            }
        }
    }
}