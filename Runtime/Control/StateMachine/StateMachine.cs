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

using System;
using System.Collections.Generic;
using UnityEngine;

namespace SBR.StateMachines {
    public interface IStateMachine {
        bool IsStateActive(string name);
        bool IsStateRemembered(string name);
        float TransitionLastTime(string from, string to);
        GameObject GameObject { get; }
    }

    public delegate void Notify();
    public delegate bool Condition();

    public class Trigger {
        bool _value;
        public void Set() => _value = true;
        public bool Get() {
            bool b = _value;
            _value = false;
            return b;
        }
    }

    public class State {
        public Notify EnterCallback;
        public Notify UpdateCallback;
        public Notify ExitCallback;

        public List<Transition> Transitions;
        public SubStateMachine SubMachine;
        public State Parent;
        public SubStateMachine ParentMachine;

        public float EnterTime = float.NegativeInfinity;

        public void EnterSelf() {
            EnterTime = Time.time;
            EnterCallback?.Invoke();
        }

        public void Enter() {
            EnterSelf();

            SubMachine?.Enter();
        }

        public void Update() {
            UpdateCallback?.Invoke();

            SubMachine?.Update();
        }

        public void Exit() {
            SubMachine?.Exit();

            ExitCallback?.Invoke();
        }
    }

    public class Transition {
        public State From;
        public State To;

        public Notify Notify;
        public Condition Cond;
        public float Cooldown;

        public float ExitTime;
        public StateMachineDefinition.TransitionMode Mode;

        public float LastTimeTakenUnscaled = float.NegativeInfinity;
        public float LastTimeTaken = float.NegativeInfinity;

        public bool IsPassable() {
            if (Time.time - LastTimeTaken < Cooldown) return false;

            if (Mode == StateMachineDefinition.TransitionMode.Time) {
                return Time.time - From.EnterTime >= ExitTime;
            } else {
                return Cond();
            }
        }
    }

    public class SubStateMachine {
        public State CurrentState;
        public State DefaultState;

        public float LeafEnterTime => ActiveLeaf.EnterTime;

        public State ActiveLeaf {
            get {
                if (CurrentState.SubMachine == null) {
                    return CurrentState;
                } else {
                    return CurrentState.SubMachine.ActiveLeaf;
                }
            }
        }

        public void Enter() {
            if (CurrentState == null) {
                CurrentState = DefaultState;
            }

            CurrentState?.Enter();
        }

        public bool Enter(Stack<State> states) {
            if (states.Count == 0) {
                Enter();
                return true;
            }

            State next = states.Pop();

            if (next.ParentMachine != this) {
                Debug.LogError("Error in transition hierarchy.");
                return false;
            } else {
                CurrentState = next;
                CurrentState.EnterSelf();

                if (states.Count > 0 && CurrentState.SubMachine == null) {
                    Debug.LogError("State in transition hierarchy doesn't have children.");
                    return false;
                } else if (CurrentState.SubMachine != null) {
                    return CurrentState.SubMachine.Enter(states);
                } else {
                    return true;
                }
            }
        }

        public void Update() {
            if (CurrentState == null) {
                Enter();
            }

            CurrentState?.Update();
        }

        public void Exit() {
            CurrentState?.Exit();
        }

        public Transition CheckTransitions() {
            if ( CurrentState == null) return null;

            foreach (Transition t in CurrentState.Transitions) {
                if (t.IsPassable()) {
                    return t;
                }
            }

            if (CurrentState.SubMachine != null) {
                return CurrentState.SubMachine.CheckTransitions();
            }

            return null;
        }

        public bool TransitionTo(Stack<State> states) {
            State next = states.Peek();

            if (next.ParentMachine != this) {
                Debug.LogError("Error in transition hierarchy on state " + next);
                return false;
            }

            if (next == CurrentState) {
                if (states.Count == 0) {
                    Debug.LogWarning("Trying to transition to already active state.");
                    return false;
                } else if (CurrentState.SubMachine == null) {
                    Debug.LogError("Trying to transition to non-existant hierarchy.");
                    return false;
                } else {
                    states.Pop();
                    return CurrentState.SubMachine.TransitionTo(states);
                }
            } else {
                Exit();
                return Enter(states);
            }
        }
    }

    public abstract class StateMachine<T> : Controller<T>, IStateMachine where T : Channels, new() {
        [NonSerialized]
        protected SubStateMachine RootMachine = new SubStateMachine();

        [NonSerialized]
        protected State[] AllStates;

        public float StateEnterTime => RootMachine.LeafEnterTime;
        public float TimeSinceEntered => Time.time - RootMachine.LeafEnterTime;
        public string StateName {
            get {
                return RootMachine.ActiveLeaf.ToString();
            }

            set {
                State state = GetState(value);
                if (state != null) {
                    TransitionTo(state);
                } else {
                    throw new ArgumentException("Invalid state name passed to stateName setter: " + value);
                }
            }
        }

        public GameObject GameObject => gameObject;

        protected State GetState(string name) {
            foreach (State s in AllStates) {
                if (s.ToString() == name) {
                    return s;
                }
            }

            return null;
        }

        public new bool IsStateActive(string name) {
            State s = RootMachine.CurrentState;
            while (s != null) {
                if (s.ToString() == name) {
                    return true;
                }

                if (s.SubMachine != null) {
                    s = s.SubMachine.CurrentState;
                } else {
                    return false;
                }
            }

            return false;
        }

        public bool IsStateRemembered(string name) {
            State s = GetState(name);
            if (s == null) {
                return false;
            }

            return s.ParentMachine.CurrentState == s || (s.ParentMachine.CurrentState == null && s.ParentMachine.DefaultState == s);
        }

        public float TransitionLastTime(string state1, string state2) {
            State s1 = GetState(state1);

            foreach (Transition t in s1.Transitions) {
                if (t.To.ToString() == state2) {
                    return t.LastTimeTakenUnscaled;
                }
            }

            return float.NegativeInfinity;
        }

        protected override void DoInput() {
            RootMachine.Update();

            Transition t = RootMachine.CheckTransitions();
            if (t != null) {
                t.Notify?.Invoke();
                t.LastTimeTakenUnscaled = Time.unscaledTime;
                t.LastTimeTaken = Time.time;
                TransitionTo(t.To);
            }
        }

        protected void TransitionTo(State target) {
            Stack<State> t = new Stack<State>();

            while (target != null) {
                t.Push(target);

                target = target.Parent;
            }

            RootMachine.TransitionTo(t);
        }

        protected Trigger TriggerOnDamage = new Trigger();
        protected virtual void OnDamage(Damage dmg) => TriggerOnDamage.Set();
    }
}
