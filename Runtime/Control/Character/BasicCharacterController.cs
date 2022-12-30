// The MIT License (MIT)
// 
// Copyright (c) 2022-present Vincent Miller
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

using Infohazard.StateSystem;
using UnityEngine;

namespace SBR {
    /// <summary>
    /// A default PlayerController for the CharacterMotor.
    /// </summary>
    public class BasicCharacterController : BasicCharacterController<CharacterChannels> { }

    /// <summary>
    /// A default PlayerController for the CharacterMotor.
    /// </summary>
    public class BasicCharacterController<T> : LegacyInputPlayerController<T> where T : CharacterChannels, new() {
        /// <summary>
        /// Minimum pitch value for channels.rotation.
        /// </summary>
        [SerializeField] private float _pitchMin = -80;

        /// <summary>
        /// Maximum pitch value for channels.rotation.
        /// </summary>
        [SerializeField] private float _pitchMax = 80;

        /// <summary>
        /// If true, input will be flattened on the world Y axis.
        /// </summary>
        [SerializeField] private bool _alignInput = true;

        [Header("Input Names")]
        [NoOverrides, SerializeField] private string _horizontalAxisName = "Horizontal";
        [NoOverrides, SerializeField] private string _verticalAxisName = "Vertical";
        [NoOverrides, SerializeField] private string _mouseXAxisName = "Mouse X";
        [NoOverrides, SerializeField] private string _mouseYAxisName = "Mouse Y";
        [NoOverrides, SerializeField] private string _jumpButtonName = "Jump";

        private Vector3 _angles;

        protected override void Awake() {
            base.Awake();
            AddAxisListener(_horizontalAxisName, Axis_Horizontal);
            AddAxisListener(_verticalAxisName, Axis_Vertical);
            AddAxisListener(_mouseXAxisName, Axis_MouseX);
            AddAxisListener(_mouseYAxisName, Axis_MouseY);
            AddButtonDownListener(_jumpButtonName, ButtonDown_Jump);
            AddButtonUpListener(_jumpButtonName, ButtonUp_Jump);
        }

        protected virtual void Axis_Horizontal(float value) {
            Vector3 right = ViewTarget ? (_alignInput ? ViewTarget.flatRight : ViewTarget.transform.right) : Vector3.right;
            Channels.Movement += right * value;
        }

        protected virtual void Axis_Vertical(float value) {
            Vector3 fwd = ViewTarget ? (_alignInput ? ViewTarget.flatForward : ViewTarget.transform.forward) : Vector3.forward;
            Channels.Movement += fwd * value;
        }

        protected virtual void ButtonDown_Jump() {
            Channels.Jump = true;
        }

        protected virtual void ButtonUp_Jump() {
            Channels.Jump = false;
        }

        protected virtual void Axis_MouseX(float value) {
            _angles.y += value;

            Channels.Rotation = Quaternion.Euler(_angles);
        }

        protected virtual void Axis_MouseY(float value) {
            _angles.x -= value;

            if (_angles.x < _pitchMin) {
                _angles.x = _pitchMin;
            } else if (_angles.x > _pitchMax) {
                _angles.x = _pitchMax;
            }

            Channels.Rotation = Quaternion.Euler(_angles);
        }
    }
}