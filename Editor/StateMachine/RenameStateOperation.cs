// The MIT License (MIT)
// 
// Copyright (c) 2026-present Val Miller
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
using UnityEditor;

namespace SBR.Editor {
    public class RenameStateOperation : Operation {
        private string name;

        public RenameStateOperation(StateMachineDefinition def, StateMachineEditorWindow window, StateMachineDefinition.State state) : base(def, window, state) {
            name = state._name;
            showBaseGUI = false;
        }

        public override void Update() {
            Event evt = Event.current;
            if (evt.type == EventType.KeyDown) {
                if (evt.keyCode == KeyCode.Return) {
                    Confirm();
                    done = true;
                } else if (evt.keyCode == KeyCode.Escape) {
                    Cancel();
                    done = true;
                }
            } else if (evt.type == EventType.MouseDown) {
                if (definition.SelectState(window.ToWorld(evt.mousePosition)) != state) {
                    Confirm();
                    done = true;
                }
            }
        }

        private void ensureNotEmpty() {
            if (name == null || name.Length == 0) {
                name = "Default";
            }
        }

        public override void Cancel() {
            if (state._name == null || state._name.Length == 0) {
                Confirm();
            } else {
                GUI.FocusControl("StateButton");
                GUIUtility.keyboardControl = 0;
            }
        }

        public override void Confirm() {
            Undo.RecordObject(definition, "Rename State");
            ensureNotEmpty();
            definition.RenameState(state, name != null ? name.Replace(" ", "") : name);
        }

        public override void OnGUI() {
            Rect rect = state.Rect;
            rect.position = window.ToScreen(rect.position);
            name = GUI.TextField(rect, name);
        }
    }
}
