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

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SBR.Editor {
    public class MoveStateOperation : Operation {
        public static int snap = 16;

        private Vector2 start;
        private Vector2 move;

        private Rect fittingParentRect;
        private Vector2 parentOrigPos;

        private Vector2 mouseLastPos;

        public List<StateMachineDefinition.State> children;

        public MoveStateOperation(StateMachineDefinition def, StateMachineEditorWindow window, StateMachineDefinition.State state) : base(def, window, state) {
            showBaseGUI = true;
            start = state.position;
            move = Vector2.zero;

            children = new List<StateMachineDefinition.State>();
            AddChildren(state);
            CalcParentRect();
        }

        private void CalcParentRect() {
            var parent = definition.GetState(state.parent);
            if (parent != null) {
                parentOrigPos = parent.position;

                fittingParentRect = parent.rect;
            }
        }

        private void AddChildren(StateMachineDefinition.State state) {
            if (state.hasChildren) {
                var c = definition.GetChildren(state.name);
                children.AddRange(c);

                foreach (var child in c) {
                    if (child != state) {
                        AddChildren(child);
                    }
                }
            }
        }

        public override void Update() {
            var evt = Event.current;
            if (evt.type == EventType.MouseUp && evt.button == 0) {
                done = true;
            } else if (evt.type == EventType.MouseDrag && evt.button == 0 && evt.mousePosition != mouseLastPos) {
                mouseLastPos = evt.mousePosition;
                move += evt.delta;

                Undo.RecordObject(definition, "Move State");

                Vector2 oldPos = state.position;
                state.position = start + move;
                Snap(ref state.position);

                Vector2 actDelta = state.position - oldPos;

                if (children != null) {
                    foreach (var child in children) {
                        child.position += actDelta;
                    }
                }
                
                StateMachineDefinition.State newParent = null;

                foreach (var s in definition.states) {
                    if (s != state && s.hasChildren && state.rect.Overlaps(s.rect)) {
                        newParent = s;
                    }
                }

                if (definition.SetStateParent(state, newParent, snap)) {
                    CalcParentRect();
                } else if (state.parent != null && state.parent.Length > 0) {
                    if (state.rect.Overlaps(fittingParentRect)) {
                        definition.FitStateToChildren(newParent, snap * 2);
                    } else {
                        var parent = definition.GetState(state.parent);
                        if (definition.SetStateParent(state, null, snap) && definition.GetChildren(parent.name).Count == 0) {
                            parent.position = parentOrigPos;
                        }
                    }
                }
                
                repaint = true;
            }

        }

        public override void Cancel() {
            state.position = start;
        }

        public override void Confirm() {
        }

        public override void OnGUI() {
        }

        public static void Snap(ref Vector2 input) {
            input.x = Mathf.Round(input.x / snap) * snap;
            input.y = Mathf.Round(input.y / snap) * snap;
        }
    }
}