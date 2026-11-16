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

using Infohazard.Core;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace SBR {
    /// <summary>
    /// Used to create an object that always faces another object, such as the camera.
    /// </summary>
    [ExecuteAlways]
    public class Billboard : MonoBehaviour {
        /// <summary>
        /// How the rotation of this object is controlled.
        /// </summary>
        [FormerlySerializedAs("mode"),Tooltip("How the rotation of this object is controlled.")]
        public Mode _mode;

        /// <summary>
        /// Whether this object faces the main camera or another object.
        /// </summary>
        [FormerlySerializedAs("targetMode"),Tooltip("Whether this object faces the main camera or another object.")]
        public TargetMode _targetMode;

        /// <summary>
        /// Whether to scale the Billboard based on distance from the target.
        /// </summary>
        [FormerlySerializedAs("scaleWithDistance"),Tooltip("Whether to scale the Billboard based on distance from the target.")]
        public bool _scaleWithDistance;

        /// <summary>
        /// Curve to follow when scaling with distance.
        /// </summary>
        [FormerlySerializedAs("scaleOverDistanceCurve"),Tooltip("Curve to follow when scaling with distance.")]
        [ConditionalDraw(nameof(_scaleWithDistance))]
        public AnimationCurve _scaleOverDistanceCurve = AnimationCurve.Constant(0, 1, 1);

        /// <summary>
        /// Whether the Billboard should function in the editor when the game isn't running.
        /// </summary>
        [FormerlySerializedAs("editorPreview"),Tooltip("Whether the Billboard should function in the editor when the game isn't running.")]
        public bool _editorPreview = true;

        /// <summary>
        /// The other object to face, if using TargetMode.TargetObject.
        /// </summary>
        [FormerlySerializedAs("targetObject"),Tooltip("The other object to face.")]
        [ConditionalDraw(nameof(_targetMode), TargetMode.TargetObject, true)]
        public Transform _targetObject;

        private void OnEnable() {
            Camera.onPreCull -= Camera_OnPreCull;
            Camera.onPreCull += Camera_OnPreCull;

            RenderPipelineManager.beginCameraRendering -= RenderPipelineManager_BeginCameraRendering;
            RenderPipelineManager.beginCameraRendering += RenderPipelineManager_BeginCameraRendering;

            Camera cam = Camera.main;
            if (cam) Camera_OnPreCull(cam);
        }

        private void RenderPipelineManager_BeginCameraRendering(ScriptableRenderContext ctx, Camera cam) {
            Camera_OnPreCull(cam);
        }

        void Camera_OnPreCull(Camera currentCamera) {
            if (!_editorPreview && !Application.isPlaying) return;

            if (_targetMode == TargetMode.Camera) {
                LookAt(currentCamera.transform);
            }
        }

        private void OnDisable() {
            Camera.onPreCull -= Camera_OnPreCull;
            RenderPipelineManager.beginCameraRendering -= RenderPipelineManager_BeginCameraRendering;
        }

        void LateUpdate() {
            if (!_editorPreview && !Application.isPlaying) return;

            if (_targetMode == TargetMode.TargetObject && _targetObject) {
                LookAt(_targetObject);
            }
        }

        private void LookAt(Transform t) {
            if (!t) return;

            if (_mode == Mode.CopyRotation) {
                transform.rotation = t.rotation;
            } else if (_mode == Mode.LookAt) {
                transform.rotation = Quaternion.LookRotation(t.position - transform.position);
            } else if (_mode == Mode.LookAtYaw) {
                Vector3 right = Vector3.Cross(Vector3.up, t.position - transform.position);
                Vector3 fwd = Vector3.Cross(right, Vector3.up).normalized;
                transform.rotation = Quaternion.LookRotation(fwd);
            }

            if (_scaleWithDistance) {
                float f = _scaleOverDistanceCurve.Evaluate(Vector3.Distance(transform.position, t.position));
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
