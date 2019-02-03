using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SBR {
    /// <summary>
    /// Used to create an object that always faces another object, such as the camera.
    /// </summary>
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
        /// The other object to face, if using TargetMode.TargetObject.
        /// </summary>
        [Tooltip("The other object to face.")]
        [Conditional("targetMode", TargetMode.TargetObject, true)]
        public Transform targetObject;

        public Transform target {
            get {
                if (targetMode == TargetMode.MainCamera) {
                    return Camera.main.transform;
                } else {
                    return targetObject;
                }
            }
        }
        
        void LateUpdate() {
            Transform t = target;

            if (mode == Mode.CopyRotation) {
                transform.rotation = t.rotation;
            } else if (mode == Mode.LookAt) {
                transform.rotation = Quaternion.LookRotation(t.position - transform.position);
            } else {
                Vector3 right = Vector3.Cross(Vector3.up, t.position - transform.position);
                Vector3 fwd = Vector3.Cross(right, Vector3.up).normalized;
                transform.rotation = Quaternion.LookRotation(fwd);
            }
        }

        public enum Mode {
            CopyRotation, LookAt, LookAtYaw
        }

        public enum TargetMode {
            MainCamera, TargetObject
        }
    }
}