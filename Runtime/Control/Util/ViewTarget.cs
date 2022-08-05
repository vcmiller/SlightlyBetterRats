// The MIT License (MIT)
// 
// Copyright (c) 2022-present Vincent Miller
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

using UnityEngine;

namespace SBR {
    /// <summary>
    /// Used to control a Camera and/or AudioListener from which a PlayerController views the world.
    /// Provides method to get aligned movement vectors.
    /// </summary>
    public class ViewTarget : MonoBehaviour {
        /// <summary>
        /// Camera that is controlled by this ViewTarget.
        /// </summary>
        new public Camera camera { get; private set; }

        /// <summary>
        /// AudioListener that is controlled by this ViewTarget.
        /// </summary>
        public AudioListener listener { get; private set; }

        /// <summary>
        /// A normalized Vector3 aligned with the ground plane for forward movement.
        /// </summary>
        public Vector3 flatForward {
            get {
                Vector3 fwd = transform.forward;
                fwd.y = 0;
                Vector3 up = transform.up;
                up.y = 0;

                if (up.sqrMagnitude > fwd.sqrMagnitude) {
                    fwd = up;
                }

                return fwd.normalized;
            }
        }

        /// <summary>
        /// A normalized vector aligned with the ground plane for right movement.
        /// </summary>
        public Vector3 flatRight => GetAlignedRight(Vector3.up);

        /// <summary>
        /// Get the forward vector of this ViewTarget, flattened and normalized on the given plane.
        /// </summary>
        /// <param name="plane">A vector perpendicular to the plane.</param>
        /// <returns>The aligned forward vector.</returns>
        public Vector3 GetAlignedForward(Vector3 plane) => GetAlignedVector(transform.forward, plane);

        /// <summary>
        /// Get the up vector of this ViewTarget, flattened and normalized on the given plane.
        /// </summary>
        /// <param name="plane">A vector perpendicular to the plane.</param>
        /// <returns>The aligned up vector.</returns>
        public Vector3 GetAlignedUp(Vector3 plane) => GetAlignedVector(transform.up, plane);

        /// <summary>
        /// Get the right vector of this ViewTarget, flattened and normalized on the given plane.
        /// </summary>
        /// <param name="plane">A vector perpendicular to the plane.</param>
        /// <returns>The aligned right vector.</returns>
        public Vector3 GetAlignedRight(Vector3 plane) => GetAlignedVector(transform.right, plane);
        
        private static Vector3 GetAlignedVector(Vector3 vector, Vector3 plane) {
            Vector3 align = Vector3.ProjectOnPlane(vector, plane);
            if (align.sqrMagnitude > 0) {
                return align.normalized;
            } else {
                return Vector3.zero;
            }
        }

        private void Awake() {
            camera = GetComponent<Camera>();
            listener = GetComponent<AudioListener>();
        }

        private void OnEnable() {
            if (camera) {
                camera.enabled = true;
            }

            if (listener) {
                listener.enabled = true;
            }
        }

        private void OnDisable() {
            if (camera) {
                camera.enabled = false;
            }

            if (listener) {
                listener.enabled = false;
            }
        }
    }
}
