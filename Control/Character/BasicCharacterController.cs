using UnityEngine;

namespace SBR {
    /// <summary>
    /// A default PlayerController for the CharacterMotor.
    /// </summary>
    public class BasicCharacterController : BasicCharacterController<CharacterChannels> { }

    /// <summary>
    /// A default PlayerController for the CharacterMotor.
    /// </summary>
    public class BasicCharacterController<T> : PlayerController<T> where T : CharacterChannels, new() {
        /// <summary>
        /// Minimum pitch value for channels.rotation.
        /// </summary>
        public float pitchMin = -80;

        /// <summary>
        /// Maximum pitch value for channels.rotation.
        /// </summary>
        public float pitchMax = 80;

        private Vector3 angles;
        
        public void Axis_Horizontal(float value) {
            Vector3 right = viewTarget ? viewTarget.flatRight : Vector3.right;
            channels.movement += right * value;
        }

        public void Axis_Vertical(float value) {
            Vector3 fwd = viewTarget ? viewTarget.flatForward : Vector3.forward;
            channels.movement += fwd * value;
        }

        public void ButtonDown_Jump() {
            channels.jump = true;
        }

        public void ButtonUp_Jump() {
            channels.jump = false;
        }

        public void Axis_MouseX(float value) {
            angles.y += value;

            channels.rotation = Quaternion.Euler(angles);
        }

        public void Axis_MouseY(float value) {
            angles.x -= value;

            if (angles.x < pitchMin) {
                angles.x = pitchMin;
            } else if (angles.x > pitchMax) {
                angles.x = pitchMax;
            }

            channels.rotation = Quaternion.Euler(angles);
        }
    }
}