using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SBR {
    // This class is extended by Externals/Math3D.cs
    public static partial class MathUtil {

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
    }
}