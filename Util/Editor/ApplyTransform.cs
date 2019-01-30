using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SBR.Editor {
    public static class ApplyTransform {
        [MenuItem("GameObject/Apply Transform/Position")]
        public static void ApplyPosition() {
            var obj = Selection.activeGameObject;
            if (obj && obj.transform.position != Vector3.zero) {
                Undo.RecordObject(obj, "Appply Position");
                Vector3[] childPositions = new Vector3[obj.transform.childCount];
                for (int i = 0; i < obj.transform.childCount; i++) {
                    childPositions[i] = obj.transform.GetChild(i).position;
                }

                obj.transform.position = Vector3.zero;

                for (int i = 0; i < obj.transform.childCount; i++) {
                    obj.transform.GetChild(i).position = childPositions[i];
                }
            }
        }

        [MenuItem("GameObject/Apply Transform/Rotation")]
        public static void ApplyRotation() {
            var obj = Selection.activeGameObject;
            if (obj && obj.transform.rotation != Quaternion.identity) {
                Undo.RecordObject(obj, "Appply Rotation");
                (Quaternion rot, Vector3 pos)[] childRotations = new (Quaternion, Vector3)[obj.transform.childCount];
                for (int i = 0; i < obj.transform.childCount; i++) {
                    var child = obj.transform.GetChild(i);
                    childRotations[i].rot = child.rotation;
                    childRotations[i].pos = child.position;
                }

                obj.transform.rotation = Quaternion.identity;

                for (int i = 0; i < obj.transform.childCount; i++) {
                    var child = obj.transform.GetChild(i);
                    child.rotation = childRotations[i].rot;
                    child.position = childRotations[i].pos;
                }
            }
        }

        [MenuItem("GameObject/Apply Transform/Scale")]
        public static void ApplyScale() {
            var obj = Selection.activeGameObject;
            if (obj && obj.transform.localScale != Vector3.one) {
                Undo.RecordObject(obj, "Appply Scale");
                Vector3 oldScale = obj.transform.localScale;
                obj.transform.localScale = Vector3.one;

                for (int i = 0; i < obj.transform.childCount; i++) {
                    var child = obj.transform.GetChild(i);
                    Vector3 v = child.transform.localPosition;
                    v.x *= oldScale.x;
                    v.y *= oldScale.y;
                    v.z *= oldScale.z;
                    child.transform.localPosition = v;

                    v = child.transform.localScale;
                    var map = GetScaleMapping(child.transform.localRotation);
                    var sr = map * oldScale;
                    v.x *= sr.x;
                    v.y *= sr.y;
                    v.z *= sr.z;
                    child.transform.localScale = v;
                }
            }
        }

        private static Quaternion GetScaleMapping(Quaternion rotation) {
            Vector3 y = rotation * Vector3.up;
            Vector3 z = rotation * Vector3.forward;

            float yx = Mathf.Abs(Vector3.Dot(y, Vector3.right));
            float yy = Mathf.Abs(Vector3.Dot(y, Vector3.up));
            float yz = Mathf.Abs(Vector3.Dot(y, Vector3.forward));

            float zx = Mathf.Abs(Vector3.Dot(z, Vector3.right));
            float zy = Mathf.Abs(Vector3.Dot(z, Vector3.up));
            float zz = Mathf.Abs(Vector3.Dot(z, Vector3.forward));

            Vector3 fwd, up;

            if (zx > zy && zx >= zz) {
                fwd = Vector3.right;
            } else if (zy > zx && zy >= zz) {
                fwd = Vector3.up;
            } else {
                fwd = Vector3.forward;
            }

            if (yx >= yy && yx > yz) {
                up = Vector3.right;
            } else if (yy >= yx && yy > yz) {
                up = Vector3.up;
            } else {
                up = Vector3.forward;
            }

            return Quaternion.LookRotation(fwd, up);
        }
        
        [MenuItem("GameObject/Apply Transform/All")]
        public static void ApplyAll() {
            ApplyPosition();
            ApplyRotation();
            ApplyScale();
        }
    }
}