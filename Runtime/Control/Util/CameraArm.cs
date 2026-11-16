// The MIT License (MIT)
// 
// Copyright (c) 2026-present Val Miller
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
using UnityEngine.Serialization;

namespace SBR {
    /// <summary>
    /// Used to create a simple third-person camera that is controlled by the player,
    /// and will avoid clipping inside geometry.
    /// </summary>
    public class CameraArm : Motor<CharacterChannels> {
        /// <summary>
        /// Whether to follow the pitch of the controller's rotation channel.
        /// </summary>
        [FormerlySerializedAs("useControlRotationX"),Tooltip("Whether to follow the pitch of the controller's rotation channel.")]
        public bool _useControlRotationX = true;

        /// <summary>
        /// Whether to follow the yaw of the controller's rotation channel.
        /// </summary>
        [FormerlySerializedAs("useControlRotationY"),Tooltip("Whether to follow the yaw of the controller's rotation channel.")]
        public bool _useControlRotationY = true;

        /// <summary>
        /// Whether to follow the roll of the controller's rotation channel.
        /// </summary>
        [FormerlySerializedAs("useControlRotationZ"),Tooltip("Whether to follow the roll of the controller's rotation channel.")]
        public bool _useControlRotationZ = false;

        /// <summary>
        /// Layers that the camera cannot be inside.
        /// </summary>
        [FormerlySerializedAs("blocking"),Tooltip("Layers that the camera cannot be inside.")]
        public LayerMask _blocking = 1;

        /// <summary>
        /// Distance that the camera will be from the player as long as it doesn't collide with anything.
        /// </summary>
        [FormerlySerializedAs("targetLength"),Tooltip("Distance that the camera will be from the player as long as it doesn't collide with anything.")]
        public float _targetLength = 6;

        [FormerlySerializedAs("cameraTransform")]
        public Transform _cameraTransform;

        [FormerlySerializedAs("cameraMovement")]
        public bool _cameraMovement = true;
        [FormerlySerializedAs("cameraRadius")]
        public float _cameraRadius = 0.1f;

        private float _lastX;
        private float _lastY;

        private Quaternion _rot;

        protected override void Awake() {
            base.Awake();

            Vector3 v = transform.eulerAngles;
            _lastX = v.x;
            _lastY = v.y;
        }

        private void UpdateCamera() {
            Vector3 v = transform.eulerAngles;
            Vector3 r = _rot.eulerAngles;

            if (_useControlRotationX) {
                v.x = r.x;
            } else {
                v.x = _lastX;
            }

            if (_useControlRotationY) {
                v.y = r.y;
            } else {
                v.y = _lastY;
            }

            if (_useControlRotationZ) {
                v.z = r.z;
            } else {
                v.z = 0;
            }

            transform.eulerAngles = v;
            _lastX = v.x;
            _lastY = v.y;

            if (_cameraTransform && _blocking != 0 && _cameraMovement) {
                RaycastHit hit;

                if (Physics.SphereCast(transform.position, _cameraRadius, -transform.forward, 
                    out hit, _targetLength + _cameraRadius, _blocking, QueryTriggerInteraction.Ignore)) {
                    _cameraTransform.transform.localPosition = new Vector3(0, 0, -hit.distance);
                } else {
                    _cameraTransform.transform.localPosition = new Vector3(0, 0, -_targetLength);
                }
            }
        }

        protected override void DoOutput(CharacterChannels c) {
            _rot = c.Rotation;
        }

        protected override void PostOutput(CharacterChannels c) {
            UpdateCamera();
        }
    }
}
