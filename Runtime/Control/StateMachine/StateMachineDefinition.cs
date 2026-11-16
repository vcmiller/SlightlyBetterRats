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
using System;
using System.Text.RegularExpressions;
using Infohazard.Core;
using UnityEngine.Serialization;

namespace SBR {
    [CreateAssetMenu(menuName = "Infohazard/State Machine")]
    public class StateMachineDefinition : ScriptableObject {
        [Serializable]
        public class State {
            /// <summary>
            /// Name of the state. Must be a valid variable name.
            /// </summary>
            [FormerlySerializedAs("name"),Tooltip("Name of the state. Must be a valid variable name.")]
            public string _name;

            /// <summary>
            /// Parent state. Must be another state in this StateMachine.
            /// </summary>
            [FormerlySerializedAs("parent"),StateSelect]
            [Tooltip("Parent state name.")]
            public string _parent;

            /// <summary>
            /// Whether the state receives a callback when it is entered.
            /// </summary>
            [FormerlySerializedAs("hasEnter"),Tooltip("Whether the state receives a callback when it is entered.")]
            public bool _hasEnter = false;

            /// <summary>
            /// Whether the state receives a callback every frame while it is active.
            /// </summary>
            [FormerlySerializedAs("hasDuring"),Tooltip("Whether the state receives a callback every frame while it is active.")]
            public bool _hasDuring = true;

            /// <summary>
            /// Whether the state receives a callback when it is exited.
            /// </summary>
            [FormerlySerializedAs("hasExit"),Tooltip("Whether the state receives a callback when it is exited.")]
            public bool _hasExit = false;

            /// <summary>
            /// Transitions to other states.
            /// </summary>
            [FormerlySerializedAs("transitions"),Tooltip("Transitions to other states.")]
            public List<Transition> _transitions;

            /// <summary>
            /// Whether the state is a sub state machine, and thus allowed to have children.
            /// </summary>
            [FormerlySerializedAs("hasChildren"),Tooltip("Whether the state is a sub state machine, and thus allowed to have children.")]
            public bool _hasChildren;

            /// <summary>
            /// Default state of the local sub state machine.
            /// </summary>
            [FormerlySerializedAs("localDefault"),StateSelect]
            [Tooltip("Default state of the local sub state machine.")]
            public string _localDefault;

            [FormerlySerializedAs("position"),HideInInspector]
            public Vector2 _position;

            [FormerlySerializedAs("size"),HideInInspector]
            public Vector2 _size = new Vector2(192, 48);

            public Rect Rect => new Rect(_position, _size);
            public Vector2 Center => _position + _size / 2;

            public Transition GetTransition(State target) {
                if (_transitions == null) {
                    return null;
                }

                return _transitions.Find((t) => {
                    return t._to == target._name;
                });
            }

            public void RemoveTransition(Transition tr) {
                _transitions.Remove(tr);
            }

            public void RemoveTransition(State target) {
                if (_transitions != null) {
                    _transitions.RemoveAll((t) => {
                        return t._to == target._name;
                    });
                }
            }

            public Transition AddTransition(State target) {
                if (target != this && GetTransition(target) == null) {
                    Transition newT = new Transition();
                    newT._to = target._name;

                    if (_transitions == null) {
                        _transitions = new List<Transition>();
                    }

                    _transitions.Add(newT);
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
            [FormerlySerializedAs("to"),StateSelect]
            [Tooltip("Which state the transition leads to.")]
            public string _to;

            /// <summary>
            /// Whether the transition receives a callback when it is taken.
            /// </summary>
            [FormerlySerializedAs("hasNotify"),Tooltip("Whether the transition receives a callback when it is taken.")]
            public bool _hasNotify = false;

            /// <summary>
            /// Minimum time after taking the transition before it can be taken again.
            /// </summary>
            [FormerlySerializedAs("cooldown"),Tooltip("Minimum time after taking the transition before it can be taken again.")]
            public float _cooldown = 0;

            /// <summary>
            /// Whether the transition is controlled by time or a condition function.
            /// </summary>
            [FormerlySerializedAs("mode"),Tooltip("Whether the transition is controlled by time or a condition function.")]
            public TransitionMode _mode = TransitionMode.Condition;

            /// <summary>
            /// Time after which the transition is exited.
            /// </summary>
            [FormerlySerializedAs("exitTime"),ConditionalDraw(nameof(_mode), TransitionMode.Condition, false)]
            [Tooltip("Time after which the transition is exited.")]
            public float _exitTime = 0.0f;

            /// <summary>
            /// Message function name that will trigger the transition.
            /// </summary>
            [FormerlySerializedAs("message"),ConditionalDraw(nameof(_mode), TransitionMode.Message)]
            [Tooltip("Message function name that will trigger the transition.")]
            public string _message = "Message";

            public int Width => 2;
        }

        public enum TransitionMode {
            Condition, Time, Message, Damage
        }

        /// <summary>
        /// Default state of the StateMachine.
        /// </summary>
        [FormerlySerializedAs("defaultState"),StateSelect]
        [Tooltip("Default state of the StateMachine.")]
        public string _defaultState;

        /// <summary>
        /// All states of the StateMachine.
        /// </summary>
        [FormerlySerializedAs("states"),Tooltip("All states of the StateMachine.")]
        public List<State> _states;

        [FormerlySerializedAs("baseClass"),TypeSelect(typeof(StateMachines.IStateMachine), true, true)]
        public string _baseClass = "SBR.StateMachines.StateMachine`1";

        public void OnValidate() {
            if (_defaultState != "" && GetState(_defaultState) == null) {
                _defaultState = "";
            }

            HashSet<string> names = new HashSet<string>();
            foreach (State state in _states) {
                state._name = Regex.Replace(state._name, "[^_a-zA-Z0-9]", "");
                string s = state._name;
                int dup = 1;
                while (!names.Add(state._name)) {
                    state._name = s + "_" + dup++;
                }

                if (state._parent != "") {
                    State parent = GetState(state._parent);
                    if (parent == null) {
                        state._parent = "";
                    } else {
                        parent._hasChildren = true;
                    }
                }

                if (state._localDefault != "") {
                    State localDefault = GetState(state._localDefault);
                    if (localDefault == null) {
                        state._localDefault = "";
                    }
                }

                foreach (Transition transition in state._transitions) {
                    if (transition._to != "" && GetState(transition._to) == null) {
                        transition._to = "";
                    }

                    if (transition._mode == TransitionMode.Message) {
                        transition._message = Regex.Replace(transition._message, "[^_a-zA-Z0-9]", "");
                        if (transition._message.Length == 0) {
                            transition._message = "Message";
                        }
                    }
                }
            }
        }

        public State GetState(string name) {
            if (name == null || name.Length == 0) {
                return null;
            }

            foreach (State def in _states) {
                if (def._name == name) {
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

            foreach (State def in _states) {
                if (def._parent == name) {
                    list.Add(def);
                }
            }

            return list;
        }

        public State SelectState(Vector2 position) {
            State ret = null;

            if (_states != null) {
                foreach (State def in _states) {
                    if (def.Rect.Contains(position)) {
                        ret = def;
                    }
                }
            }

            return ret;
        }

        public Pair<Vector2, Vector2> GetTransitionPoints(State from, Transition t) {
            State to = GetState(t._to);

            Rect r1 = from.Rect;
            Rect r2 = to.Rect;

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

            src += ortho * t.Width;
            dest += ortho * t.Width;

            return new Pair<Vector2, Vector2>(src, dest);
        }

        public Pair<State, Transition> SelectTransition(Vector2 position) {
            if (_states != null) {
                foreach (State from in _states) {
                    if (from._transitions != null) {
                        foreach (Transition tr in from._transitions) {
                            if (GetState(tr._to) == null) {
                                continue;
                            }

                            Pair<Vector2, Vector2> line = GetTransitionPoints(from, tr);

                            Vector2 src = line.Item1;
                            Vector2 dest = line.Item2;

                            Vector2 v = (dest - src).normalized;

                            float angle = Mathf.Atan2(v.y, v.x);

                            Vector2 pr = position - src;

                            Quaternion q = Quaternion.Euler(0, 0, -angle * Mathf.Rad2Deg);

                            pr = q * pr;

                            Rect rect = new Rect(0, -tr.Width * 1.5f, Vector2.Distance(src, dest), tr.Width * 3);

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
            if (_states == null) {
                _states = new List<State>();
            }

            State newState = new State();
            _states.Add(newState);
            return newState;
        }

        public void RemoveState(State toRemove) {
            foreach (State s in _states) {
                s.RemoveTransition(toRemove);
            }

            _states.Remove(toRemove);
        }

        public void RenameState(State toRename, string newName) {
            State existing;
            string uniqueName = newName;
            int dup = 0;

            while ((existing = GetState(uniqueName)) != null && existing != toRename) {
                uniqueName = newName + "_" + ++dup;
            }

            foreach (State s in _states) {
                Transition t = s.GetTransition(toRename);
                if (t != null) {
                    t._to = uniqueName;
                }
            }

            foreach (State child in GetChildren(toRename._name)) {
                child._parent = uniqueName;
            }

            if (_defaultState == toRename._name) {
                _defaultState = uniqueName;
            }

            toRename._name = uniqueName;
        }

        public void MoveAfter(State moving, State target) {
            int iMoving = _states.IndexOf(moving);
            int iTarget = _states.IndexOf(target);

            if (iMoving >= 0 && iTarget >= 0 && iMoving < iTarget) {
                _states.RemoveAt(iMoving);
                _states.Insert(iTarget, moving);

                foreach (State child in GetChildren(moving._name)) {
                    MoveAfter(child, moving);
                }
            }
        }

        public void RemoveSub(State toConvert, float padding) {
            toConvert._hasChildren = false;
            toConvert._size = new Vector2(192, 48);

            foreach (State child in GetChildren(toConvert._name)) {
                child._parent = null;
            }

            if (!string.IsNullOrEmpty(toConvert._parent)) {
                FitStateToChildren(GetState(toConvert._parent), padding);
            }
        }

        public void CreateSub(State toConvert, float padding) {
            toConvert._hasChildren = true;
            FitStateToChildren(toConvert, padding);
        }

        public bool IsAncestor(State ancestor, State descendant) {
            if (ancestor == null || descendant == null) {
                return false;
            }

            if (ancestor == descendant) {
                return true;
            }

            if (descendant._parent == null || descendant._parent.Length == 0) {
                return false;
            }

            return IsAncestor(ancestor, GetState(descendant._parent));
        }

        public bool SetStateParent(State state, State newParent, float padding) {
            State oldParent = GetState(state._parent);
            if (oldParent != newParent && !IsAncestor(state, newParent)) {
                if (oldParent != null) {
                    state._parent = null;
                    FitStateToChildren(oldParent, padding * 2);
                }

                if (newParent != null) {
                    state._parent = newParent._name;
                    MoveAfter(state, newParent);
                    FitStateToChildren(newParent, padding * 2);
                } else {
                    state._parent = null;
                }

                return true;
            } else {
                return false;
            }
        }

        public void FitStateToChildren(State state, float padding) {
            if (!state._hasChildren) {
                return;
            }

            state._size = new Vector2(192, 96);
            List<State> children = GetChildren(state._name);

            if (children.Count > 0) {
                state._position = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
            }

            foreach (State child in children) {
                state._position.x = Mathf.Min(state._position.x, child._position.x - padding);
                state._position.y = Mathf.Min(state._position.y, child._position.y - padding * 2);
            }

            foreach (State child in children) {
                state._size.x = Mathf.Max(state._size.x, child._position.x + child._size.x + padding - state._position.x);
                state._size.y = Mathf.Max(state._size.y, child._position.y + child._size.y + padding - state._position.y);
            }

            State parent = GetState(state._parent);

            if (parent != null && parent != state) {
                FitStateToChildren(parent, padding);
            }
        }
    }
}
