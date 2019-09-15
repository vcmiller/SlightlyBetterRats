using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SBR.Editor {
    [CustomEditor(typeof(AttachToBone))]
    public class AttachToBoneInspector : UnityEditor.Editor {
        private static readonly string[] excludeProperties = new[]{ "m_Script", "_localPosition", "_localRotation", "_localScale" };

        public override void OnInspectorGUI() {
            var attach = target as AttachToBone;

            EditorGUI.BeginChangeCheck();
            DrawDefaultInspector();
            bool changed = EditorGUI.EndChangeCheck();

            bool reset = GUILayout.Button("Reset Local Transform");
            if (reset) {
                Undo.RecordObjects(new Object[] { attach, attach.transform }, "Reset Local Transform");
                attach.LocalPosition = Vector3.zero;
                attach.LocalEulerAngles = Vector3.zero;
                attach.LocalScale = Vector3.one;
                EditorUtility.SetDirty(attach);
            }

            if (changed || reset) {
                Undo.RecordObject(attach.transform, "Edit Property Value");
                attach.UpdateTransformFromValues();
            }
        }
    }

}