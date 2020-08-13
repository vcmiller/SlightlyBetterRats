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
using UnityEngine;

using Complex = System.Numerics.Complex;

namespace SBR {
    // This class is extended by Externals/Math3D.cs
    public static partial class MathUtil {
        #region Float Operations

        /// <summary>
        /// Round a value to the nearest multiple of factor.
        /// </summary>
        /// <param name="angle"></param>
        /// <param name="multiple"></param>
        /// <returns></returns>
        public static float RoundToNearest(float value, float factor) {
            return Mathf.Round(value / factor) * factor;
        }

        /// <summary>
        /// Same as Mathf.Sign, except that if the input is zero, it returns zero.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static float SignZero(float value) {
            if (value > 0) return 1;
            if (value < 0) return -1;
            else return 0;
        }

        /// <summary>
        /// Evaluate all cubic roots of this <c>Complex</c>.
        /// Modified from https://github.com/mathnet/mathnet-numerics/blob/master/src/Numerics/ComplexExtensions.cs
        /// </summary>
        public static (Complex, Complex, Complex) ComplexCubeRoot(Complex complex) {
            double r = Math.Pow(complex.Magnitude, 1.0 / 3.0);
            double theta = complex.Phase / 3.0;
            const double shift = Math.PI * 2.0 / 3.0;
            return (Complex.FromPolarCoordinates(r, theta),
                    Complex.FromPolarCoordinates(r, theta + shift),
                    Complex.FromPolarCoordinates(r, theta - shift));
        }

        public static (Complex r1, Complex r2) SolveQuadratic(Complex a, Complex b, Complex c) {
            Complex sqrt = Complex.Sqrt(b * b - 4 * a * c);
            Complex denom = 2 * a;

            return ((-b + sqrt) / denom, (-b - sqrt) / denom);
        }

        public static (Complex r1, Complex r2, Complex r3) SolveCubic(Complex a, Complex b, Complex c, Complex d) {
            Complex delta0 = (b * b) - (3 * a * c);
            Complex delta1 = (2 * b * b * b) - (9 * a * b * c) + (27 * a * a * d);

            Complex root = Complex.Sqrt((delta1 * delta1) - (4 * delta0 * delta0 * delta0));

            double epsilon = 0.00001;
            if (Math.Abs(delta1.Real - root.Real) < epsilon && Math.Abs(delta1.Imaginary - root.Imaginary) < epsilon) {
                root = -root;
            }

            var (C1, C2, C3) = ComplexCubeRoot((delta1 - root) / 2.0);

            Complex r1 = -(1.0 / (3.0 * a)) * (b + C1 + (delta0 / C1));
            Complex r2 = -(1.0 / (3.0 * a)) * (b + C2 + (delta0 / C2));
            Complex r3 = -(1.0 / (3.0 * a)) * (b + C3 + (delta0 / C3));

            return (r1, r2, r3);
        }

        public static (Complex r1, Complex r2, Complex r3, Complex r4) SolveQuartic(Complex a, Complex b, Complex c, Complex d, Complex e) {
            // https://math.stackexchange.com/a/57688

            Complex A = (c / a) - ((3 * b * b) / (8 * a * a));
            Complex B = (d / a) - ((b * c) / (2 * a * a)) + ((b * b * b) / (8 * a * a * a));
            Complex C = (e / a) - ((b * d) / (4 * a * a)) + ((b * b * c) / (16 * a * a * a)) - ((3 * b * b * b * b) / (256 * a * a * a * a));

            var (s1, s2, s3) = SolveCubic(8.0, -4.0 * A, -8.0 * C, (4.0 * A * C) - (B * B));

            Complex cmp1 = 0.5 * Complex.Sqrt((2.0 * s1) - A);
            Complex cmp2 = (-2.0 * s1) - A;
            Complex cmp3 = (2.0 * B) / Complex.Sqrt((2.0 * s1) - A);
            Complex cmp4 = -b / (4.0 * a);

            Complex sqrt1 = 0.5 * Complex.Sqrt(cmp2 + cmp3) + cmp4;
            Complex sqrt2 = 0.5 * Complex.Sqrt(cmp2 - cmp3) + cmp4;

            Complex r1 = -cmp1 + sqrt1;
            Complex r2 = -cmp1 - sqrt1;
            Complex r3 =  cmp1 + sqrt2;
            Complex r4 =  cmp1 - sqrt2;

            return (r1, r2, r3, r4);
        }

        #endregion

        #region Angle Operations

        /// <summary>
        /// Normalize an angle to a value between 0 and 360.
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        public static float NormalizeAngle(float angle) {
            return ((angle % 360) + 360) % 360;
        }

        public static Vector3 NormalizeAngles(Vector3 angles) {
            return new Vector3(
                NormalizeAngle(angles.x),
                NormalizeAngle(angles.y),
                NormalizeAngle(angles.z));
        }

        /// <summary>
        /// Normalize an angle to a value between -180 and 180.
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        public static float NormalizeInnerAngle(float angle) {
            float result = NormalizeAngle(angle);
            if (result > 180) {
                result -= 360;
            }
            return result;
        }

        public static Vector3 NormalizeInnerAngles(Vector3 angles) {
            return new Vector3(
                NormalizeInnerAngle(angles.x),
                NormalizeInnerAngle(angles.y),
                NormalizeInnerAngle(angles.z));
        }

        /// <summary>
        /// Normalize an angle to a value between -180 and 180, then clamp it in the given range.
        /// </summary>
        /// <param name="angle"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static float ClampInnerAngle(float angle, float min, float max) {
            return Mathf.Clamp(NormalizeInnerAngle(angle), min, max);
        }

        public static Vector3 ClampInnerAngles(Vector3 angles, Vector3 min, Vector3 max) {
            return new Vector3(
                ClampInnerAngle(angles.x, min.x, max.x),
                ClampInnerAngle(angles.y, min.y, max.y),
                ClampInnerAngle(angles.z, min.z, max.z));
        }

        #endregion

        #region Vector Operations

        /// <summary>
        /// Multiply the components of left by right. This is not a mathematically valid operation, but it can still be useful.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Vector3 Multiply(Vector3 left, Vector3 right) {
            return new Vector3(left.x * right.x, left.y * right.y, left.z * right.z);
        }

        /// <summary>
        /// Divide the components of left by right. This is not a mathematically valid operation, but it can still be useful.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Vector3 Divide(Vector3 left, Vector3 right) {
            return new Vector3(left.x / right.x, left.y / right.y, left.z / right.z);
        }

        /// <summary>
        /// Take the reciprocal of each component of a vector. This is not a mathematically valid operation, but it can still be useful.
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static Vector3 Reciprocal(Vector3 vector) => Divide(1.0f, vector);

        /// <summary>
        /// Divide a float by each component of a vector. This is not a mathematically valid operation, but it can still be useful.
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static Vector3 Divide(float left, Vector3 right) {
            return new Vector3(left / right.x, left / right.y, left / right.z);
        }

        /// <summary>
        /// Round each component of vector to the nearest multiple of factor.
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="factor"></param>
        /// <returns></returns>
        public static Vector3 RoundToNearest(Vector3 vector, float factor) {
            return new Vector3(
                RoundToNearest(vector.x, factor),
                RoundToNearest(vector.y, factor),
                RoundToNearest(vector.z, factor)
                );
        }

        public static bool GetNearestPointOnLines(Ray line1, Ray line2, out float t1, out float t2) {
            // https://stackoverflow.com/a/2316934
            // mua = ( d1343 d4321 - d1321 d4343 ) / ( d2121 d4343 - d4321 d4321 )

            Vector3 p1 = line1.origin;
            Vector3 v1 = line1.direction;
            Vector3 p2 = line2.origin;
            Vector3 v2 = line2.direction;

            float d(Vector3 a, Vector3 b) => Vector3.Dot(a, b);

            t1 = ((d(p1 - p2, v2) * d(v2, v1)) - (d(p1 - p2, v1) * d(v2, v2))) / 
                 ((d(v1, v1)      * d(v2, v2)) - (d(v2, v1)      * d(v2, v1)));

            t2 = (d(p1 - p2, v2) + t1 * d(v2, v1)) / d(v2, v2);

            return !float.IsNaN(t1) && !float.IsNaN(t2) && !float.IsInfinity(t1) && !float.IsInfinity(t2);
        }

        public static float GetNearestPointOnLine(Ray line, Vector3 p) {
            Vector3 v = p - line.origin;
            return Vector3.Dot(line.direction, v);
        }

        #endregion

        #region Rect Operations

        /// <summary>
        /// Split a rect into two halves horizontally, with given gap between the halves.
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="gap"></param>
        /// <param name="out1"></param>
        /// <param name="out2"></param>
        /// <param name="div"></param>
        public static void SplitHorizontal(Rect rect, float gap, out Rect out1, out Rect out2, float div = 0.5f) {
            gap /= 2;
            out1 = new Rect(rect.x, rect.y, rect.width * div - gap, rect.height);
            out2 = new Rect(out1.xMax + gap * 2, rect.y, rect.width * (1 - div) - gap, rect.height);
        }

        /// <summary>
        /// Split a rect into three halves horizontally, with given gap between the halves.
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="gap"></param>
        /// <param name="out1"></param>
        /// <param name="out2"></param>
        /// <param name="out3"></param>
        /// <param name="div1"></param>
        /// <param name="div2"></param>
        public static void SplitHorizontal(Rect rect, float gap, out Rect out1, out Rect out2, out Rect out3, float div1 = 1.0f / 3.0f, float div2 = 2.0f / 3.0f) {
            gap /= 2;
            out1 = new Rect(rect.x, rect.y, rect.width * div1 - gap, rect.height);
            out2 = new Rect(out1.xMax + gap * 2, rect.y, rect.width * (div2 - div1) - gap, rect.height);
            out3 = new Rect(out2.xMax + gap * 2, rect.y, rect.width * (1 - (div1 + div2)) - gap, rect.height);
        }

        #endregion
    }
}