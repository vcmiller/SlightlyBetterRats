using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SBR {
    /// <summary>
    /// A Component representing a spline in world space, with a nice editor.
    /// </summary>
    public class Spline : MonoBehaviour {
        /// <summary>
        /// Underlying spline data.
        /// </summary>
        public SplineData spline = new SplineData();

        /// <summary>
        /// Invoked when the spline changes and depending scripts, such as spline mesh, must update.
        /// </summary>
        public event Action<bool> PropertyChanged;

        /// <summary>
        /// Get a point on the spline in world space.
        /// </summary>
        /// <param name="pos">Position on the spline, in the range [0, 1].</param>
        /// <returns>The world space point on the spline.</returns>
        public Vector3 GetWorldPoint(float pos) {
            return transform.TransformPoint(spline.GetPoint(pos));
        }

        /// <summary>
        /// Get a tangent of the spline in world space.
        /// </summary>
        /// <param name="pos">Position on the spline, in the range [0, 1].</param>
        /// <returns>The world space tangent on the spline.</returns>
        public Vector3 GetWoldTangent(float pos) {
            return transform.TransformVector(spline.GetTangent(pos));
        }

        /// <summary>
        /// Get a rotation of the spline in world space.
        /// </summary>
        /// <param name="pos">Position on the spline, in the range [0, 1].</param>
        /// <returns>The world space rotation on the spline.</returns>
        public Quaternion GetWorldRotation(float pos) {
            return transform.rotation * spline.GetRotation(pos);
        }

        /// <summary>
        /// Fill an array with uniformly spaced samples along the spline.
        /// </summary>
        /// <param name="samples">The array to fill.</param>
        public void GetWorldPoints(Vector3[] samples) {
            for (int i = 0; i < samples.Length; i++) {
                samples[i] = GetWorldPoint(i / (samples.Length - 1.0f));
            }
        }

        private void OnValidate() {
            OnChanged(false);
        }

        /// <summary>
        /// Called by the editor when properties of the spline are changed.
        /// </summary>
        /// <param name="update">Whether to update depending components immediately.</param>
        public void OnChanged(bool update) {
            if (spline == null) {
                return;
            }

            spline.InvalidateSamples();

            PropertyChanged?.Invoke(update);
        }
    }

}