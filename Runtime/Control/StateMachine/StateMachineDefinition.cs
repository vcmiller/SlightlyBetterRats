﻿// The MIT License (MIT)
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

using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text.RegularExpressions;
using Infohazard.Core;

namespace SBR {
    [CreateAssetMenu(menuName = "Infohazard/State Machine")]
    public class StateMachineDefinition : ScriptableObject {
        [Serializable]
        public class State {
            /// <summary>
            /// Name of the state. Must be a valid variable name.
            /// </summary>
            [Tooltip("Name of the state. Must be a valid variable name.")]
            public string name;

            /// <summary>
            /// Parent state. Must be another state in this StateMachine.
            /// </summary>
            [StateSelect]
            [Tooltip("Parent state name.")]
            public string parent;

            /// <summary>
            /// Whether the state receives a callback when it is entered.
            /// </summary>
            [Tooltip("Whether the state receives a callback when it is entered.")]
            public bool hasEnter = false;

            /// <summary>
            /// Whether the state receives a callback every frame while it is active.
            /// </summary>
            [Tooltip("Whether the state receives a callback every frame while it is active.")]
            public bool hasDuring = true;

            /// <summary>
            /// Whether the state receives a callback when it is exited.
            /// </summary>
            [Tooltip("Whether the state receives a callback when it is exited.")]
            public bool hasExit = false;

            /// <summary>
            /// Transitions to other states.
            /// </summary>
            [Tooltip("Transitions to other states.")]
            public List<Transition> transitions;
            
            /// <summary>
            /// Whether the state is a sub state machine, and thus allowed to have children.
            /// </summary>
            [Tooltip("Whether the state is a sub state machine, and thus allowed to have children.")]
            public bool hasChildren;

            /// <summary>
            /// Default state of the local sub state machine.
            /// </summary>
            [StateSelect]
            [Tooltip("Default state of the local sub state machine.")]
            public string localDefault;

            [HideInInspector]
            public Vector2 position;

            [HideInInspector]
            public Vector2 size = new Vector2(192, 48);

            public Rect rect => new Rect(position, size);
            public Vector2 center => position + size / 2;

            public Transition GetTransition(State target) {
                if (transitions == null) {
                    return null;
                }

                return transitions.Find((t) => {
                    return t.to == target.name;
                });
            }

            public void RemoveTransition(Transition tr) {
                transitions.Remove(tr);
            }

            public void RemoveTransition(State target) {
                if (transitions != null) {
                    transitions.RemoveAll((t) => {
                        return t.to == target.name;
                    });
                }
            }

            public Transition AddTransition(State target) {
                if (target != this && GetTransition(target) == null) {
                    Transition newT = new Transition();
                    newT.to = target.name;

                    if (transitions == null) {
                        transitions = new List<Transition>();
                    }

                    transitions.Add(newT);
                    return newT;
                } else {
                    return null;
                }
            }
        }

        [Serializable]
        public class Transition {
            /// <summary>
            /// Which state the transition leads to.
            /// </summary>
            [StateSelect]
            [Tooltip("Which state the transition leads to.")]
            public string to;

            /// <summary>
            /// Whether the transition receives a callback when it is taken.
            /// </summary>
            [Tooltip("Whether the transition receives a callback when it is taken.")]
            public bool hasNotify = false;

            /// <summary>
            /// Minimum time after taking the transition before it can be taken again.
            /// </summary>
            [Tooltip("Minimum time after taking the transition before it can be taken again.")]
            public float cooldown = 0;

            /// <summary>
            /// Whether the transition is controlled by time or a condition function.
            /// </summary>
            [Tooltip("Whether the transition is controlled by time or a condition function.")]
            public TransitionMode mode = TransitionMode.Condition;

            /// <summary>
            /// Time after which the transition is exited.
            /// </summary>
            [ConditionalDraw("mode", TransitionMode.Condition, false)]
            [Tooltip("Time after which the transition is exited.")]
            public float exitTime = 0.0f;

            /// <summary>
            /// Message function name that will trigger the transition.
            /// </summary>
            [ConditionalDraw("mode", TransitionMode.Message)]
            [Tooltip("Message function name that will trigger the transition.")]
            public string message = "Message";

            public int width => 2;
        }

        public enum TransitionMode {
            Condition, Time, Message, Damage
        }

        /// <summary>
        /// Default state of the StateMachine.
        /// </summary>
        [StateSelect]
        [Tooltip("Default state of the StateMachine.")]
        public string defaultState;

        /// <summary>
        /// All states of the StateMachine.
        /// </summary>
        [Tooltip("All states of the StateMachine.")]
        public List<State> states;

        [TypeSelect(typeof(StateMachines.IStateMachine), true, true)]
        public string baseClass = "SBR.StateMachines.StateMachine`1";

        public void OnValidate() {
            if (defaultState != "" && GetState(defaultState) == null) {
                defaultState = "";
            }

            HashSet<string> names = new HashSet<string>();
            foreach (var state in states) {
                state.name = Regex.Replace(state.name, "[^_a-zA-Z0-9]", "");
                string s = state.name;
                int dup = 1;
                while (!names.Add(state.name)) {
                    state.name = s + "_" + dup++;
                }

                if (state.parent != "") {
                    var parent = GetState(state.parent);
                    if (parent == null) {
                        state.parent = "";
                    } else {
                        parent.hasChildren = true;
                    }
                }

                if (state.localDefault != "") {
                    var localDefault = GetState(state.localDefault);
                    if (localDefault == null) {
                        state.localDefault = "";
                    }
                }

                foreach (var transition in state.transitions) {
                    if (transition.to != "" && GetState(transition.to) == null) {
                        transition.to = "";
                    }

                    if (transition.mode == TransitionMode.Message) {
                        transition.message = Regex.Replace(transition.message, "[^_a-zA-Z0-9]", "");
                        if (transition.message.Length == 0) {
                            transition.message = "Message";
                        }
                    }
                }
            }
        }
        
        public State GetState(string name) {
            if (name == null || name.Length == 0) {
                return null;
            }

            foreach (State def in states) {
                if (def.name == name) {
                    return def;
                }
            }

            return null;
        }

        public List<State> GetChildren(string name) {
            List<State> list = new List<State>();

            if (name == null || name.Length == 0) {
                return list;
            }

            foreach (State def in states) {
                if (def.parent == name) {
                    list.Add(def);
                }
            }

            return list;
        }

        public State SelectState(Vector2 position) {
            State ret = null;

            if (states != null) {
                foreach (State def in states) {
                    if (def.rect.Contains(position)) {
                        ret = def;
                    }
                }
            }

            return ret;
        }

        public Pair<Vector2, Vector2> GetTransitionPoints(State from, Transition t) {
            State to = GetState(t.to);

            Rect r1 = from.rect;
            Rect r2 = to.rect;

            Vector2 src, dest;

            if (r1.xMax < r2.xMin) {
                src.x = r1.xMax;
                dest.x = r2.xMin;
            } else if (r2.xMax < r1.xMin) {
                src.x = r1.xMin;
                dest.x = r2.xMax;
            } else {
                float w1 = r1.width;
                float w2 = r2.width;
                src.x = dest.x = (r1.center.x * w2 + r2.center.x * w1) / (w1 + w2);
            }
            
            if (r1.yMax < r2.yMin) {
                src.y = r1.yMax;
                dest.y = r2.yMin;
            } else if (r2.yMax < r1.yMin) {
                src.y = r1.yMin;
                dest.y = r2.yMax;
            } else {
                float w1 = r1.height;
                float w2 = r2.height;
                src.y = dest.y = (r1.center.y * w2 + r2.center.y * w1) / (w1 + w2);
            }

            Vector2 v = dest - src;
            v = v.normalized;
            Vector2 ortho = new Vector2(v.y, -v.x);

            src += ortho * t.width;
            dest += ortho * t.width;

            return new Pair<Vector2, Vector2>(src, dest);
        }

        public Pair<State, Transition> SelectTransition(Vector2 position) {
            if (states != null) {
                foreach (State from in states) {
                    if (from.transitions != null) {
                        foreach (Transition tr in from.transitions) {
                            if (GetState(tr.to) == null) {
                                continue;
                            }

                            var line = GetTransitionPoints(from, tr);

                            Vector2 src = line.Item1;
                            Vector2 dest = line.Item2;

                            Vector2 v = (dest - src).normalized;

                            float angle = Mathf.Atan2(v.y, v.x);

                            Vector2 pr = position - src;

                            Quaternion q = Quaternion.Euler(0, 0, -angle * Mathf.Rad2Deg);

                            pr = q * pr;

                            Rect rect = new Rect(0, -tr.width * 1.5f, Vector2.Distance(src, dest), tr.width * 3);

                            if (rect.Contains(pr)) {
                                return new Pair<State, Transition>(from, tr);
                            }
                        }
                    }
                }
            }
            

            return new Pair<State, Transition>(null, null);
        }

        public bool TransitionValid(State from, State to) {
            if (from.GetTransition(to) != null) {
                return false;
            }

            if (IsAncestor(from, to) || IsAncestor(to, from)) {
                return false;
            }

            return true;
        }

        public State AddState() {
            if (states == null) {
                states = new List<State>();
            }

            State newState = new State();
            states.Add(newState);
            return newState;
        }

        public void RemoveState(State toRemove) {
            foreach (State s in states) {
                s.RemoveTransition(toRemove);
            }

            states.Remove(toRemove);
        }

        public void RenameState(State toRename, string newName) {
            State existing;
            string uniqueName = newName;
            int dup = 0;

            while ((existing = GetState(uniqueName)) != null && existing != toRename) {
                uniqueName = newName + "_" + ++dup;
            }

            foreach (State s in states) {
                var t = s.GetTransition(toRename);
                if (t != null) {
                    t.to = uniqueName;
                }
            }

            foreach (var child in GetChildren(toRename.name)) {
                child.parent = uniqueName;
            }

            if (defaultState == toRename.name) {
                defaultState = uniqueName;
            }

            toRename.name = uniqueName;
        }

        public void MoveAfter(State moving, State target) {
            int iMoving = states.IndexOf(moving);
            int iTarget = states.IndexOf(target);

            if (iMoving >= 0 && iTarget >= 0 && iMoving < iTarget) {
                states.RemoveAt(iMoving);
                states.Insert(iTarget, moving);

                foreach (var child in GetChildren(moving.name)) {
                    MoveAfter(child, moving);
                }
            }
        }

        public void RemoveSub(State toConvert, float padding) {
            toConvert.hasChildren = false;
            toConvert.size = new Vector2(192, 48);

            foreach (var child in GetChildren(toConvert.name)) {
                child.parent = null;
            }

            if (!string.IsNullOrEmpty(toConvert.parent)) {
                FitStateToChildren(GetState(toConvert.parent), padding);
            }
        }

        public void CreateSub(State toConvert, float padding) {
            toConvert.hasChildren = true;
            FitStateToChildren(toConvert, padding);
        }

        public bool IsAncestor(State ancestor, State descendant) {
            if (ancestor == null || descendant == null) {
                return false;
            }

            if (ancestor == descendant) {
                return true;
            }

            if (descendant.parent == null || descendant.parent.Length == 0) {
                return false;
            }

            return IsAncestor(ancestor, GetState(descendant.parent));
        }

        public bool SetStateParent(State state, State newParent, float padding) {
            State oldParent = GetState(state.parent);
            if (oldParent != newParent && !IsAncestor(state, newParent)) {
                if (oldParent != null) {
                    state.parent = null;
                    FitStateToChildren(oldParent, padding * 2);
                }

                if (newParent != null) {
                    state.parent = newParent.name;
                    MoveAfter(state, newParent);
                    FitStateToChildren(newParent, padding * 2);
                } else {
                    state.parent = null;
                }

                return true;
            } else {
                return false;
            }
        }

        public void FitStateToChildren(State state, float padding) {
            if (!state.hasChildren) {
                return;
            }

            state.size = new Vector2(192, 96);
            var children = GetChildren(state.name);

            if (children.Count > 0) {
                state.position = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
            }

            foreach (var child in children) {
                state.position.x = Mathf.Min(state.position.x, child.position.x - padding);
                state.position.y = Mathf.Min(state.position.y, child.position.y - padding * 2);
            }

            foreach (var child in children) {
                state.size.x = Mathf.Max(state.size.x, child.position.x + child.size.x + padding - state.position.x);
                state.size.y = Mathf.Max(state.size.y, child.position.y + child.size.y + padding - state.position.y);
            }

            var parent = GetState(state.parent);

            if (parent != null && parent != state) {
                FitStateToChildren(parent, padding);
            }
        }
    }
}