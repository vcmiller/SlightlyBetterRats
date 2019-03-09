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
        protected override void Awake() {
            base.Awake();
            AddAxisListener("Horizontal", Axis_Horizontal);
            AddButtonDownListener("Jump", ButtonDown_Jump);
            AddButtonUpListener("Jump", ButtonUp_Jump);
        }

        private void Axis_Horizontal(float value) {
            Vector3 right = viewTarget ? viewTarget.flatRight : Vector3.right;
            channels.movement += right * value;
        }

        private void ButtonDown_Jump() {
            channels.jump = true;
        }

        private void ButtonUp_Jump() {
            channels.jump = false;
        }
    }
}
