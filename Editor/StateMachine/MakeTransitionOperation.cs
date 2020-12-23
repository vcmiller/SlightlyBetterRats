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
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SBR.Editor {
    public class MakeTransitionOperation : Operation {
        private Vector2 target;

        public MakeTransitionOperation(StateMachineDefinition def, StateMachineEditorWindow window, StateMachineDefinition.State state) : base(def, window, state) {
            showBaseGUI = true;
        }

        public override void Update() {
            var evt = Event.current;
            if (evt.type == EventType.MouseMove) {
                target = evt.mousePosition;
                repaint = true;
            } else if (evt.type == EventType.MouseDown) {
                var targ = definition.SelectState(window.ToWorld(target));

                if (evt.button == 0 && targ != null && targ != state) {
                    done = true;
                    Confirm();
                } else {
                    done = true;
                    Cancel();
                }
            }
        }

        public override void Cancel() {
        }

        public override void Confirm() {
            Undo.RecordObject(definition, "Add Transition");
            var targ = definition.SelectState(window.ToWorld(target));

            if (definition.TransitionValid(state, targ)) {
                state.AddTransition(targ);
            }
        }

        public override void OnGUI() {
            Handles.BeginGUI();

            Vector2 src = window.ToScreen(state.center);

            var targ = definition.SelectState(window.ToWorld(target));

            if (targ == null) {
                Handles.color = Color.red;
                Handles.DrawAAPolyLine(3, src, target);
            } else if (targ != state) {
                if (definition.TransitionValid(state, targ)) {
                    Handles.color = Color.black;
                } else {
                    Handles.color = Color.red;
                }

                var fake = new StateMachineDefinition.Transition { to = targ.name };
                var line = definition.GetTransitionPoints(state, fake);

                Handles.DrawAAPolyLine(3, window.ToScreen(line.Item1), window.ToScreen(line.Item2));
            }

            Handles.EndGUI();
        }
    }
}