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
        /// Layers that the camera cannot be inside.
        /// </summary>
        [Tooltip("Layers that the camera cannot be inside.")]
        public LayerMask blocking = 1;

        /// <summary>
        /// Distance that the camera will be from the player as long as it doesn't collide with anything.
        /// </summary>
        [Tooltip("Distance that the camera will be from the player as long as it doesn't collide with anything.")]
        public float targetLength = 6;

        private float lastX;
        private float lastY;

        private Quaternion rot;
        private Camera cam;

        protected override void Awake() {
            base.Awake();

            Vector3 v = transform.eulerAngles;
            lastX = v.x;
            lastY = v.y;

            cam = GetComponentInChildren<Camera>();
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

            v.z = 0;

            transform.eulerAngles = v;
            lastX = v.x;
            lastY = v.y;

            if (cam && blocking != 0) {
                RaycastHit hit;

                if (Physics.SphereCast(transform.position, cam.nearClipPlane, -transform.forward, out hit, targetLength + cam.nearClipPlane, blocking)) {
                    cam.transform.localPosition = new Vector3(0, 0, -hit.distance);
                } else {
                    cam.transform.localPosition = new Vector3(0, 0, -targetLength);
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