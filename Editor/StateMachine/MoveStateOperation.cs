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
            start = state._position;
            move = Vector2.zero;

            children = new List<StateMachineDefinition.State>();
            AddChildren(state);
            CalcParentRect();
        }

        private void CalcParentRect() {
            StateMachineDefinition.State parent = definition.GetState(state._parent);
            if (parent != null) {
                parentOrigPos = parent._position;

                fittingParentRect = parent.Rect;
            }
        }

        private void AddChildren(StateMachineDefinition.State state) {
            if (state._hasChildren) {
                List<StateMachineDefinition.State> c = definition.GetChildren(state._name);
                children.AddRange(c);

                foreach (StateMachineDefinition.State child in c) {
                    if (child != state) {
                        AddChildren(child);
                    }
                }
            }
        }

        public override void Update() {
            Event evt = Event.current;
            if (evt.type == EventType.MouseUp && evt.button == 0) {
                done = true;
            } else if (evt.type == EventType.MouseDrag && evt.button == 0 && evt.mousePosition != mouseLastPos) {
                mouseLastPos = evt.mousePosition;
                move += evt.delta;

                Undo.RecordObject(definition, "Move State");

                Vector2 oldPos = state._position;
                state._position = start + move;
                Snap(ref state._position);

                Vector2 actDelta = state._position - oldPos;

                if (children != null) {
                    foreach (StateMachineDefinition.State child in children) {
                        child._position += actDelta;
                    }
                }
                
                StateMachineDefinition.State newParent = null;

                foreach (StateMachineDefinition.State s in definition._states) {
                    if (s != state && s._hasChildren && state.Rect.Overlaps(s.Rect)) {
                        newParent = s;
                    }
                }

                if (definition.SetStateParent(state, newParent, snap)) {
                    CalcParentRect();
                } else if (state._parent != null && state._parent.Length > 0) {
                    if (state.Rect.Overlaps(fittingParentRect)) {
                        definition.FitStateToChildren(newParent, snap * 2);
                    } else {
                        StateMachineDefinition.State parent = definition.GetState(state._parent);
                        if (definition.SetStateParent(state, null, snap) && definition.GetChildren(parent._name).Count == 0) {
                            parent._position = parentOrigPos;
                        }
                    }
                }
                
                repaint = true;
            }

        }

        public override void Cancel() {
            state._position = start;
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
