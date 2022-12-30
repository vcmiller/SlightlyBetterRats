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

using UnityEngine;

namespace SBR {
    /// <summary>
    /// A default PlayerController for the CharacterMotor2D.
    /// </summary>
    public class BasicCharacterController2D : BasicCharacterController2D<CharacterChannels> { }

    /// <summary>
    /// A default PlayerController for the CharacterMotor2D.
    /// </summary>
    public class BasicCharacterController2D<T> : LegacyInputPlayerController<T> where T : CharacterChannels, new() {
        protected override void Awake() {
            base.Awake();
            AddAxisListener("Horizontal", Axis_Horizontal);
            AddButtonDownListener("Jump", ButtonDown_Jump);
            AddButtonUpListener("Jump", ButtonUp_Jump);
        }

        private void Axis_Horizontal(float value) {
            Vector3 right = ViewTarget ? ViewTarget.flatRight : Vector3.right;
            Channels.Movement += right * value;
        }

        private void ButtonDown_Jump() {
            Channels.Jump = true;
        }

        private void ButtonUp_Jump() {
            Channels.Jump = false;
        }
    }
}
