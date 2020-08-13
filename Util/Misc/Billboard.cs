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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SBR {
    /// <summary>
    /// Used to create an object that always faces another object, such as the camera.
    /// </summary>
    [ExecuteAlways]
    public class Billboard : MonoBehaviour {
        /// <summary>
        /// How the rotation of this object is controlled.
        /// </summary>
        [Tooltip("How the rotation of this object is controlled.")]
        public Mode mode;

        /// <summary>
        /// Whether this object faces the main camera or another object.
        /// </summary>
        [Tooltip("Whether this object faces the main camera or another object.")]
        public TargetMode targetMode;

        /// <summary>
        /// Whether to scale the Billboard based on distance from the target.
        /// </summary>
        [Tooltip("Whether to scale the Billboard based on distance from the target.")]
        public bool scaleWithDistance;

        /// <summary>
        /// Curve to follow when scaling with distance.
        /// </summary>
        [Tooltip("Curve to follow when scaling with distance.")]
        [Conditional(nameof(scaleWithDistance))]
        public AnimationCurve scaleOverDistanceCurve = AnimationCurve.Constant(0, 1, 1);

        /// <summary>
        /// Whether the Billboard should function in the editor when the game isn't running.
        /// </summary>
        [Tooltip("Whether the Billboard should function in the editor when the game isn't running.")]
        public bool editorPreview = true;

        /// <summary>
        /// The other object to face, if using TargetMode.TargetObject.
        /// </summary>
        [Tooltip("The other object to face.")]
        [Conditional(nameof(targetMode), TargetMode.TargetObject, true)]
        public Transform targetObject;

        private void OnEnable() {
            Camera.onPreCull -= Camera_OnPreCull;
            Camera.onPreCull += Camera_OnPreCull;
        }

        void Camera_OnPreCull(Camera camera) {
            if (!editorPreview && !Application.isPlaying) return;

            if (targetMode == TargetMode.Camera) {
                LookAt(camera.transform);
            }
        }

        private void OnDisable() {
            Camera.onPreCull -= Camera_OnPreCull;
        }

        void LateUpdate() {
            if (!editorPreview && !Application.isPlaying) return;

            if (targetMode == TargetMode.TargetObject && targetObject) {
                LookAt(targetObject);
            }
        }

        private void LookAt(Transform t) {
            if (!t) return;

            if (mode == Mode.CopyRotation) {
                transform.rotation = t.rotation;
            } else if (mode == Mode.LookAt) {
                transform.rotation = Quaternion.LookRotation(t.position - transform.position);
            } else if (mode == Mode.LookAtYaw) {
                Vector3 right = Vector3.Cross(Vector3.up, t.position - transform.position);
                Vector3 fwd = Vector3.Cross(right, Vector3.up).normalized;
                transform.rotation = Quaternion.LookRotation(fwd);
            }

            if (scaleWithDistance) {
                float f = scaleOverDistanceCurve.Evaluate(Vector3.Distance(transform.position, t.position));
                transform.localScale = Vector3.one * f;
            }
        }

        public enum Mode {
            CopyRotation, LookAt, LookAtYaw, NoRotation
        }

        public enum TargetMode {
            Camera, TargetObject
        }
    }
}