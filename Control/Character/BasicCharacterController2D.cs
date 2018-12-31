using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SBR {
    /// <summary>
    /// A default PlayerController for the CharacterMotor2D.
    /// </summary>
    public class BasicCharacterController2D : BasicCharacterController2D<CharacterChannels> { }

    /// <summary>
    /// A default PlayerController for the CharacterMotor2D.
    /// </summary>
    public class BasicCharacterController2D<T> : PlayerController<T> where T : CharacterChannels, new() {

        public void Axis_Horizontal(float value) {
            Vector3 right = viewTarget.transform.right;
            right.y = 0;
            right = right.normalized;

            channels.movement += right * value;
        }

        public void ButtonDown_Jump() {
            channels.jump = true;
        }

        public void ButtonUp_Jump() {
            channels.jump = false;
        }
    }
}
