using UnityEngine;
using System.Collections;
using UnityEngine.Audio;
using System.Text.RegularExpressions;

namespace SBR {
    /// <summary>
    /// Contains several utility functions.
    /// </summary>
    public static class Util {
        /// <summary>
        /// A more versatile version of AudioSource.PlayClipAtPoint.
        /// </summary>
        /// <param name="clip">The clip to play.</param>
        /// <param name="point">The point at which to play.</param>
        /// <param name="volume">The volume to play at.</param>
        /// <param name="spatial">Spatial blend (0 = 2D, 1 = 3D).</param>
        /// <param name="pitch">The pitch to play at.</param>
        /// <param name="loop">Whether to loop the sound.</param>
        /// <param name="output">Output mixer group.</param>
        /// <param name="attach">Transform to attach the spawned AudioSource to.</param>
        /// <returns>The spawned AudioSource.</returns>
        public static AudioSource PlayClipAtPoint(AudioClip clip, Vector3 point, float volume = 1, float spatial = 1, float pitch = 1, bool loop = false, AudioMixerGroup output = null, Transform attach = null) {
            if (clip == null) {
                return null;
            }

            GameObject obj = new GameObject();
            obj.name = "One shot audio (SBR)";
            obj.transform.parent = attach;
            obj.transform.position = point;

            var src = obj.AddComponent<AudioSource>();
            src.clip = clip;
            src.loop = loop;
            src.spatialBlend = spatial;
            src.volume = volume;
            src.pitch = pitch;
            src.outputAudioMixerGroup = output;
            src.Play();

            if (!loop) {
                Object.Destroy(obj, clip.length);
            }

            return src;
        }

        /// <summary>
        /// Draw the given Bounds in the scene view for one frame.
        /// </summary>
        /// <param name="bounds">Bounds to draw.</param>
        /// <param name="color">Color to draw.</param>
        public static void DrawDebugBounds(Bounds bounds, Color color) {
            Debug.DrawLine(new Vector3(bounds.min.x, bounds.min.y, bounds.min.z), new Vector3(bounds.max.x, bounds.min.y, bounds.min.z), color);
            Debug.DrawLine(new Vector3(bounds.min.x, bounds.max.y, bounds.min.z), new Vector3(bounds.max.x, bounds.max.y, bounds.min.z), color);
            Debug.DrawLine(new Vector3(bounds.min.x, bounds.max.y, bounds.max.z), new Vector3(bounds.max.x, bounds.max.y, bounds.max.z), color);
            Debug.DrawLine(new Vector3(bounds.min.x, bounds.min.y, bounds.max.z), new Vector3(bounds.max.x, bounds.min.y, bounds.max.z), color);
            Debug.DrawLine(new Vector3(bounds.min.x, bounds.min.y, bounds.min.z), new Vector3(bounds.min.x, bounds.max.y, bounds.min.z), color);
            Debug.DrawLine(new Vector3(bounds.max.x, bounds.min.y, bounds.min.z), new Vector3(bounds.max.x, bounds.max.y, bounds.min.z), color);
            Debug.DrawLine(new Vector3(bounds.max.x, bounds.min.y, bounds.max.z), new Vector3(bounds.max.x, bounds.max.y, bounds.max.z), color);
            Debug.DrawLine(new Vector3(bounds.min.x, bounds.min.y, bounds.max.z), new Vector3(bounds.min.x, bounds.max.y, bounds.max.z), color);
            Debug.DrawLine(new Vector3(bounds.min.x, bounds.min.y, bounds.min.z), new Vector3(bounds.min.x, bounds.min.y, bounds.max.z), color);
            Debug.DrawLine(new Vector3(bounds.max.x, bounds.min.y, bounds.min.z), new Vector3(bounds.max.x, bounds.min.y, bounds.max.z), color);
            Debug.DrawLine(new Vector3(bounds.max.x, bounds.max.y, bounds.min.z), new Vector3(bounds.max.x, bounds.max.y, bounds.max.z), color);
            Debug.DrawLine(new Vector3(bounds.min.x, bounds.max.y, bounds.min.z), new Vector3(bounds.min.x, bounds.max.y, bounds.max.z), color);
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
            Vector3 capsuleCenter = capsule.transform.TransformPoint(capsule.center);
            Vector3 capsuleUp;
            float scaleY;
            float scaleXZ;

            if (capsule.direction == 0) {
                capsuleUp = capsule.transform.right;
                scaleY = capsule.transform.lossyScale.x;
                scaleXZ = Mathf.Max(Mathf.Abs(capsule.transform.lossyScale.y), Mathf.Abs(capsule.transform.lossyScale.z));
            } else if (capsule.direction == 1) {
                capsuleUp = capsule.transform.up;
                scaleY = capsule.transform.lossyScale.y;
                scaleXZ = Mathf.Max(Mathf.Abs(capsule.transform.lossyScale.x), Mathf.Abs(capsule.transform.lossyScale.z));
            } else {
                capsuleUp = capsule.transform.forward;
                scaleY = capsule.transform.lossyScale.z;
                scaleXZ = Mathf.Max(Mathf.Abs(capsule.transform.lossyScale.x), Mathf.Abs(capsule.transform.lossyScale.y));
            }

            radius = scaleXZ * capsule.radius;

            height = Mathf.Max(scaleY * capsule.height, radius * 2);

            float h = height / 2 - radius;

            point1 = capsuleCenter + capsuleUp * h;
            point2 = capsuleCenter - capsuleUp * h;
        }

        /// <summary>
        /// Splits a camel-case string into words separated by spaces. 
        /// Multiple consecutive capitals are considered the same word.
        /// </summary>
        /// <remarks>
        /// From stackoverflow:
        /// https://stackoverflow.com/questions/5796383/insert-spaces-between-words-on-a-camel-cased-token/5796793
        /// </remarks>
        /// <param name="str">The string to split.</param>
        /// <returns>The split string.</returns>
        public static string SplitCamelCase(this string str) {
            return Regex.Replace(
                Regex.Replace(
                    str,
                    @"(\P{Ll})(\P{Ll}\p{Ll})",
                    "$1 $2"
                ),
                @"(\p{Ll})(\P{Ll})",
                "$1 $2"
            );
        }

        /// <summary>
        /// Like GetComponent, but more convenient if using in if statements and also using the component value.
        /// </summary>
        public static bool TryGetComponent<T>(this GameObject obj, out T result) {
            T cmp = obj.GetComponent<T>();
            result = cmp;
            return cmp != null;
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
        /// Like GetComponent, but more convenient if using in if statements and also using the component value.
        /// </summary>
        public static bool TryGetComponent<T>(this Component cmp, out T result) {
            return cmp.gameObject.TryGetComponent(out result);
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