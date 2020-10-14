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

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SBR {
    /// <summary>
    /// Used to create a simple third-person camera that is controlled by the player,
    /// and will avoid clipping inside geometry.
    /// </summary>
    public class CameraArm : Motor<CharacterChannels> {
        /// <summary>
        /// Whether to follow the pitch of the controller's rotation channel.
        /// </summary>
        [Tooltip("Whether to follow the pitch of the controller's rotation channel.")]
        public bool useControlRotationX = true;

        /// <summary>
        /// Whether to follow the yaw of the controller's rotation channel.
        /// </summary>
        [Tooltip("Whether to follow the yaw of the controller's rotation channel.")]
        public bool useControlRotationY = true;

        /// <summary>
        /// Whether to follow the roll of the controller's rotation channel.
        /// </summary>
        [Tooltip("Whether to follow the roll of the controller's rotation channel.")]
        public bool useControlRotationZ = false;

        /// <summary>
        /// Layers that the camera cannot be inside.
        /// </summary>
        [Tooltip("Layers that the camera cannot be inside.")]
        public LayerMask blocking = 1;

        /// <summary>
        /// Distance that the camera will be from the player as long as it doesn't collide with anything.
        /// </summary>
        [Tooltip("Distance that the camera will be from the player as long as it doesn't collide with anything.")]
        public float targetLength = 6;

        public Transform cameraTransform;

        public bool cameraMovement = true;

        private float lastX;
        private float lastY;

        private Quaternion rot;
        private Camera camera;

        protected override void Awake() {
            base.Awake();

            Vector3 v = transform.eulerAngles;
            lastX = v.x;
            lastY = v.y;

            camera = GetComponentInChildren<Camera>();
            if (!cameraTransform) cameraTransform = camera.transform;
        }

        private void UpdateCamera() {
            Vector3 v = transform.eulerAngles;
            Vector3 r = rot.eulerAngles;

            if (useControlRotationX) {
                v.x = r.x;
            } else {
                v.x = lastX;
            }

            if (useControlRotationY) {
                v.y = r.y;
            } else {
                v.y = lastY;
            }

            if (useControlRotationZ) {
                v.z = r.z;
            } else {
                v.z = 0;
            }

            transform.eulerAngles = v;
            lastX = v.x;
            lastY = v.y;

            if (cameraTransform && blocking != 0 && cameraMovement) {
                RaycastHit hit;

                if (Physics.SphereCast(transform.position, camera.nearClipPlane, -transform.forward, 
                    out hit, targetLength + camera.nearClipPlane, blocking)) {
                    cameraTransform.transform.localPosition = new Vector3(0, 0, -hit.distance);
                } else {
                    cameraTransform.transform.localPosition = new Vector3(0, 0, -targetLength);
                }
            }
        }

        protected override void DoOutput(CharacterChannels c) {
            rot = c.rotation;
        }

        protected override void PostOutput(CharacterChannels c) {
            UpdateCamera();
        }
    }
}