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

using SBR.StateSystem;
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

        /// <summary>
        /// If true, input will be flattened on the world Y axis.
        /// </summary>
        public bool alignInput = true;

        [Header("Input Names")]
        [NoOverrides, SerializeField] protected string horizontalAxisName = "Horizontal";
        [NoOverrides, SerializeField] protected string verticalAxisName = "Vertical";
        [NoOverrides, SerializeField] protected string mouseXAxisName = "Mouse X";
        [NoOverrides, SerializeField] protected string mouseYAxisName = "Mouse Y";
        [NoOverrides, SerializeField] protected string jumpButtonName = "Jump";

        private Vector3 angles;

        protected override void Awake() {
            base.Awake();
            AddAxisListener(horizontalAxisName, Axis_Horizontal);
            AddAxisListener(verticalAxisName, Axis_Vertical);
            AddAxisListener(mouseXAxisName, Axis_MouseX);
            AddAxisListener(mouseYAxisName, Axis_MouseY);
            AddButtonDownListener(jumpButtonName, ButtonDown_Jump);
            AddButtonUpListener(jumpButtonName, ButtonUp_Jump);
        }

        protected virtual void Axis_Horizontal(float value) {
            Vector3 right = viewTarget ? (alignInput ? viewTarget.flatRight : viewTarget.transform.right) : Vector3.right;
            channels.movement += right * value;
        }

        protected virtual void Axis_Vertical(float value) {
            Vector3 fwd = viewTarget ? (alignInput ? viewTarget.flatForward : viewTarget.transform.forward) : Vector3.forward;
            channels.movement += fwd * value;
        }

        protected virtual void ButtonDown_Jump() {
            channels.jump = true;
        }

        protected virtual void ButtonUp_Jump() {
            channels.jump = false;
        }

        protected virtual void Axis_MouseX(float value) {
            angles.y += value;

            channels.rotation = Quaternion.Euler(angles);
        }

        protected virtual void Axis_MouseY(float value) {
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