using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SBR {
    public static class ComponentUtil {
        public static void GetCapsuleInfo(float radius, float height, Vector3 center, int direction, Transform transform,
            out Vector3 point1, out Vector3 point2, out float worldRadius, out float worldHeight) {
            Vector3 capsuleCenter = transform.TransformPoint(center);
            Vector3 capsuleUp;
            float scaleY;
            float scaleXZ;

            if (direction == 0) {
                capsuleUp = transform.right;
                scaleY = transform.lossyScale.x;
                scaleXZ = Mathf.Max(Mathf.Abs(transform.lossyScale.y), Mathf.Abs(transform.lossyScale.z));
            } else if (direction == 1) {
                capsuleUp = transform.up;
                scaleY = transform.lossyScale.y;
                scaleXZ = Mathf.Max(Mathf.Abs(transform.lossyScale.x), Mathf.Abs(transform.lossyScale.z));
            } else {
                capsuleUp = transform.forward;
                scaleY = transform.lossyScale.z;
                scaleXZ = Mathf.Max(Mathf.Abs(transform.lossyScale.x), Mathf.Abs(transform.lossyScale.y));
            }

            worldRadius = scaleXZ * radius;

            worldHeight = Mathf.Max(scaleY * height, worldRadius * 2);

            float h = worldHeight / 2 - worldRadius;

            point1 = capsuleCenter + capsuleUp * h;
            point2 = capsuleCenter - capsuleUp * h;
        }

        public static void GetCapsuleInfo(this CharacterController capsule, out Vector3 point1, out Vector3 point2, out float radius, out float height) {
            GetCapsuleInfo(capsule.radius, capsule.height, capsule.center, 1, capsule.transform, out point1, out point2, out radius, out height);
        }

        /// <summary>
        /// Get the points of a capsule, as well as radius and height, in world space.
        /// Primarily used for calls to Physics.CapsuleCast.
        /// </summary>
        /// <param name="capsule">The CapsuleCollider.</param>
        /// <param name="point1">The top point of the capsule.</param>
        /// <param name="point2">The bottom point of the capsule.</param>
        /// <param name="radius">The radius of the capsule.</param>
        /// <param name="height">The height of the capsule.</param>
        public static void GetCapsuleInfo(this CapsuleCollider capsule, out Vector3 point1, out Vector3 point2, out float radius, out float height) {
            GetCapsuleInfo(capsule.radius, capsule.height, capsule.center, capsule.direction, capsule.transform, out point1, out point2, out radius, out height);
        }

        /// <summary>
        /// Set the parent of the given transform, and reset it's local position, rotation, and scale.
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="parent"></param>
        public static void SetParentAndReset(this Transform transform, Transform parent) {
            transform.parent = parent;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }

        /// <summary>
        /// Like GetComponentInParent, but more convenient if using in if statements and also using the component value.
        /// </summary>
        public static bool TryGetComponentInParent<T>(this GameObject obj, out T result) {
            T cmp = obj.GetComponentInParent<T>();
            result = cmp;
            return cmp != null;
        }

        /// <summary>
        /// Like GetComponentInChildren, but more convenient if using in if statements and also using the component value.
        /// </summary>
        public static bool TryGetComponentInChildren<T>(this GameObject obj, out T result) {
            T cmp = obj.GetComponentInChildren<T>();
            result = cmp;
            return cmp != null;
        }

        /// <summary>
        /// Like GetComponentInParent, but more convenient if using in if statements and also using the component value.
        /// </summary>
        public static bool TryGetComponentInParent<T>(this Component cmp, out T result) {
            return cmp.gameObject.TryGetComponentInParent(out result);
        }

        /// <summary>
        /// Like GetComponentInChildren, but more convenient if using in if statements and also using the component value.
        /// </summary>
        public static bool TryGetComponentInChildren<T>(this Component cmp, out T result) {
            return cmp.gameObject.TryGetComponentInChildren(out result);
        }
    }
}
