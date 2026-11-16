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

// Some code in this class is adapted from Cinemachine

namespace SBR {
    /// <summary>
    /// Class for storing the data of a spline and performing spline calculations.
    /// This does not give you a nice editor. If you want a GameObject contains a spline,
    /// use the Spline component instead.
    /// </summary>
    [System.Serializable]
    public class SplineData {
        /// <summary>
        /// A control point on the spline.
        /// </summary>
        [System.Serializable]
        public class Point {
            [FormerlySerializedAs("position")]
            public Vector3 _position;
            [FormerlySerializedAs("tangent")]
            public Vector3 _tangent;
            [FormerlySerializedAs("roll")]
            public float _roll;
        }

        /// <summary>
        /// Whether the spline is a circuit.
        /// </summary>
        [FormerlySerializedAs("closed")]
        public bool _closed;

        /// <summary>
        /// The points of the spline.
        /// </summary>
        [FormerlySerializedAs("points")]
        public Point[] _points = new Point[0];

        private float[] _samples = new float[100];
        private bool _samplesDirty = true;
        private float _length;

        /// <summary>
        /// The minimum position on the spline (always 0).
        /// </summary>
        public float Min { get { return 0; } }

        /// <summary>
        /// The maximum non-normalized position on the spline.
        /// </summary>
        public float Max { get { return Mathf.Max(0, _closed ? _points.Length : _points.Length - 1); } }

        /// <summary>
        /// Number of samples for spline calculations.
        /// </summary>
        public int SampleCount {
            get { return _samples.Length; }
            set {
                if (value != _samples.Length) {
                    _samples = new float[value];
                    InvalidateSamples();
                }
            }
        }

        /// <summary>
        /// Non-normalized length of the spline.
        /// </summary>
        public float Length {
            get {
                RefreshSamples();
                return _length;
            }
        }

        /// <summary>
        /// Invalidate calculated samples so they are recalculated later.
        /// </summary>
        public void InvalidateSamples() {
            _samplesDirty = true;
        }

        private void RefreshSamples() {
            if (!_samplesDirty) return;

            Vector3 lastP = GetPointNonUniform(0);
            _length = 0;
            _samples[0] = 0;
            for (int i = 1; i < _samples.Length; i++) {
                Vector3 curP = GetPointNonUniform(i / (_samples.Length - 1.0f));
                _length += Vector3.Distance(curP, lastP);
                _samples[i] = _length;
                lastP = curP;
            }

            for (int i = 0; i < _samples.Length; i++) {
                _samples[i] /= _length;
            }

            _samplesDirty = false;
        }

        private float Normalize(float pos) {
            if (pos > 1 || pos < -1) {
                pos %= 1;
            }

            if (pos < 0) {
                pos++;
            }

            return pos;
        }

        private float ToNonUniform(float pos) {
            pos = Normalize(pos);
            RefreshSamples();

            for (int i = 0; i < _samples.Length - 1; i++) {
                if (_samples[i] <= pos && _samples[i + 1] >= pos) {
                    float f = (pos - _samples[i]) / (_samples[i + 1] - _samples[i]);
                    return Mathf.Lerp(i, i + 1, f) / (_samples.Length - 1.0f);
                }
            }

            return Mathf.Clamp01(pos);
        }

        private float ScaleToLength(float pos) {
            if (_closed) {
                return pos * _points.Length;
            } else {
                return pos * Mathf.Max(0, _points.Length - 1);
            }
        }

        private float GetSegment(float pos, out int indexA, out int indexB) {
            pos = ScaleToLength(Normalize(pos));
            int rounded = Mathf.RoundToInt(pos);
            if (Mathf.Abs(pos - rounded) < 0.0001f) {
                indexA = indexB = (rounded == _points.Length) ? 0 : rounded;
            } else {
                indexA = Mathf.FloorToInt(pos);
                indexB = Mathf.CeilToInt(pos);
                if (indexB >= _points.Length) {
                    indexB = 0;
                }
            }
            return pos;
        }
        
        /// <summary>
        /// Get the rotation of a control point.
        /// </summary>
        /// <param name="index">The control point.</param>
        /// <returns>The rotation of the control point.</returns>
        public Quaternion GetPointRotation(int index) {
            Quaternion q = Quaternion.identity;

            Vector3 lastFwd = Vector3.forward;

            for (int i = 0; i <= index; i++) {
                Vector3 fwd = _points[i]._tangent;
                Quaternion temp = Quaternion.FromToRotation(lastFwd, fwd);
                q = temp * q;
                lastFwd = fwd;
            }

            return q;
        }

        /// <summary>
        /// Get a point along the spline. Position will change at different rates at different parts of the spline.
        /// </summary>
        /// <param name="pos">The non-uniform position in range [0, 1].</param>
        /// <returns>The point on the spline.</returns>
        public Vector3 GetPointNonUniform(float pos) {
            if (_points.Length == 0)
                return Vector3.zero;
            else {
                int indexA, indexB;
                pos = GetSegment(pos, out indexA, out indexB);
                if (indexA == indexB) {
                    return _points[indexA]._position;
                } else {
                    Point ptA = _points[indexA];
                    Point ptB = _points[indexB];
                    float t = pos - indexA;
                    float d = 1f - t;
                    Vector3 ctrl1 = ptA._position + ptA._tangent;
                    Vector3 ctrl2 = ptB._position - ptB._tangent;
                    return d * d * d * ptA._position + 3f * d * d * t * ctrl1
                        + 3f * d * t * t * ctrl2 + t * t * t * ptB._position;
                }
            }
        }

        /// <summary>
        /// Get a point along the spline.
        /// </summary>
        /// <param name="pos">The position in range [0, 1].</param>
        /// <returns>The point on the spline.</returns>
        public Vector3 GetPoint(float pos) {
            return GetPointNonUniform(ToNonUniform(pos));
        }

        /// <summary>
        /// Get a tangent along the spline. Position will change at different rates at different parts of the spline.
        /// </summary>
        /// <param name="pos">The non-uniform position in range [0, 1].</param>
        /// <returns>The tangent on the spline.</returns>
        public Vector3 GetTangentNonUniform(float pos) {
            if (_points.Length == 0)
                return Vector3.forward;
            else {
                int indexA, indexB;
                pos = GetSegment(pos, out indexA, out indexB);
                if (indexA == indexB) {
                    return _points[indexA]._tangent;
                } else {
                    Point ptA = _points[indexA];
                    Point ptB = _points[indexB];
                    float t = pos - indexA;
                    Vector3 ctrl1 = ptA._position + ptA._tangent;
                    Vector3 ctrl2 = ptB._position - ptB._tangent;
                    return Vector3.Normalize((-3f * ptA._position + 9f * ctrl1 - 9f * ctrl2 + 3f * ptB._position) * t * t
                        + (6f * ptA._position - 12f * ctrl1 + 6f * ctrl2) * t
                        - 3f * ptA._position + 3f * ctrl1);
                }
            }
        }

        /// <summary>
        /// Get a tangent along the spline.
        /// </summary>
        /// <param name="pos">The position in range [0, 1].</param>
        /// <returns>The tangent on the spline.</returns>
        public Vector3 GetTangent(float pos) {
            return GetTangentNonUniform(ToNonUniform(pos));
        }

        /// <summary>
        /// Get a roll along the spline. Position will change at different rates at different parts of the spline.
        /// </summary>
        /// <param name="pos">The non-uniform position in range [0, 1].</param>
        /// <returns>The roll on the spline.</returns>
        public float GetRollNonUniform(float pos) {
            if (_points.Length == 0) {
                return 0;
            } else {
                int indexA, indexB;
                pos = GetSegment(pos, out indexA, out indexB);
                if (indexA == indexB) {
                    return _points[indexA]._roll;
                } else {
                    float rollA = _points[indexA]._roll;
                    float rollB = _points[indexB]._roll;
                    if (indexB == 0) {
                        rollA = rollA % 360;
                        rollB = rollB % 360;
                    }
                    return Mathf.Lerp(rollA, rollB, pos - indexA);
                }
            }
        }

        /// <summary>
        /// Get a roll along the spline.
        /// </summary>
        /// <param name="pos">The position in range [0, 1].</param>
        /// <returns>The roll on the spline.</returns>
        public float GetRoll(float pos) {
            return GetRollNonUniform(ToNonUniform(pos));
        }

        /// <summary>
        /// Get a rotation along the spline. Position will change at different rates at different parts of the spline.
        /// </summary>
        /// <param name="pos">The non-uniform position in range [0, 1].</param>
        /// <returns>The rotation on the spline.</returns>
        public Quaternion GetRotationNonUniform(float pos) {
            if (_points.Length == 0) {
                return Quaternion.identity;
            } else {
                int indexA, indexB;
                GetSegment(pos, out indexA, out indexB);
                float roll = GetRollNonUniform(pos);
                Vector3 fwd = GetTangentNonUniform(pos);

                if (fwd.sqrMagnitude > 0.0001f) {
                    Quaternion from = GetPointRotation(indexA);
                    Quaternion q = Quaternion.FromToRotation(_points[indexA]._tangent, fwd) * from;
                    return q * Quaternion.AngleAxis(roll, Vector3.forward);
                } else {
                    return Quaternion.identity;
                }
            }
        }

        /// <summary>
        /// Get a rotation along the spline.
        /// </summary>
        /// <param name="pos">The position in range [0, 1].</param>
        /// <returns>The rotation on the spline.</returns>
        public Quaternion GetRotation(float pos) {
            return GetRotationNonUniform(ToNonUniform(pos));
        }
    }
}
