using System;
using System.Collections.Generic;
using UnityEngine;

namespace SBR.StateMachines {
    public interface IStateMachine {
        bool IsStateActive(string name);
        bool IsStateRemembered(string name);
        float TransitionLastTime(string from, string to);
        GameObject gameObject { get; }
    }

    public delegate void Notify();
    public delegate bool Condition();

    public class Trigger {
        bool value;
        public void Set() => value = true;
        public bool Get() {
            bool b = value;
            value = false;
            return b;
        }
    }

    public class State {
        public Notify enter;
        public Notify during;
        public Notify exit;

        public List<Transition> transitions;
        public SubStateMachine subMachine;
        public State parent;
        public SubStateMachine parentMachine;

        public float enterTime = float.NegativeInfinity;

        public void EnterSelf() {
            enterTime = Time.time;
            enter?.Invoke();
        }

        public void Enter() {
            EnterSelf();

            if (subMachine != null) {
                subMachine.Enter();
            }
        }

        public void Update() {
            during?.Invoke();

            if (subMachine != null) {
                subMachine.Update();
            }
        }

        public void Exit() {
            if (subMachine != null) {
                subMachine.Exit();
            }

            exit?.Invoke();
        }
    }

    public class Transition {
        public State from;
        public State to;

        public Notify notify;
        public Condition cond;
        public float cooldown;

        public float exitTime;
        public StateMachineDefinition.TransitionMode mode;

        public float lastTimeTakenUnscaled = float.NegativeInfinity;
        public float lastTimeTaken = float.NegativeInfinity;

        public bool IsPassable() {
            if (Time.time - lastTimeTaken < cooldown) return false;
            
            if (mode == StateMachineDefinition.TransitionMode.Time) {
                return Time.time - from.enterTime >= exitTime;
            } else {
                return cond();
            } 
        }
    }

    public class SubStateMachine {
        public State currentState;
        public State defaultState;

        public float leafEnterTime => activeLeaf.enterTime;

        public State activeLeaf {
            get {
                if (currentState.subMachine == null) {
                    return currentState;
                } else {
                    return currentState.subMachine.activeLeaf;
                }
            }
        }

        public void Enter() {
            if (currentState == null) {
                currentState = defaultState;
            }

            currentState?.Enter();
        }

        public bool Enter(Stack<State> states) {
            if (states.Count == 0) {
                Enter();
                return true;
            }

            var next = states.Pop();

            if (next.parentMachine != this) {
                Debug.LogError("Error in transition hierarchy.");
                return false;
            } else {
                currentState = next;
                currentState.EnterSelf();

                if (states.Count > 0 && currentState.subMachine == null) {
                    Debug.LogError("State in transition hierarchy doesn't have children.");
                    return false;
                } else if (currentState.subMachine != null) {
                    return currentState.subMachine.Enter(states);
                } else {
                    return true;
                }
            }
        }

        public void Update() {
            if (currentState == null) {
                Enter();
            }

            currentState?.Update();
        }

        public void Exit() {
            currentState?.Exit();
        }

        public Transition CheckTransitions() {
            if ( currentState == null) return null;

            foreach (var t in currentState.transitions) {
                if (t.IsPassable()) {
                    return t;
                }
            }

            if (currentState.subMachine != null) {
                return currentState.subMachine.CheckTransitions();
            }

            return null;
        }

        public bool TransitionTo(Stack<State> states) {
            var next = states.Peek();

            if (next.parentMachine != this) {
                Debug.LogError("Error in transition hierarchy on state " + next);
                return false;
            }

            if (next == currentState) {
                if (states.Count == 0) {
                    Debug.LogWarning("Trying to transition to already active state.");
                    return false;
                } else if (currentState.subMachine == null) {
                    Debug.LogError("Trying to transition to non-existant hierarchy.");
                    return false;
                } else {
                    states.Pop();
                    return currentState.subMachine.TransitionTo(states);
                }
            } else {
                Exit();
                return Enter(states);
            }
        }
    }

    public abstract class StateMachine<T> : Controller<T>, IStateMachine where T : Channels, new() {
        [NonSerialized]
        protected SubStateMachine rootMachine = new SubStateMachine();

        [NonSerialized]
        protected State[] allStates;

        public float stateEnterTime => rootMachine.leafEnterTime;
        public float timeSinceEntered => Time.time - rootMachine.leafEnterTime;
        public string stateName {
            get { 
                return rootMachine.activeLeaf.ToString();
            }

            set {
                var state = GetState(value);
                if (state != null) {
                    TransitionTo(state);
                } else {
                    throw new ArgumentException("Invalid state name passed to stateName setter: " + value);
                }
            }
        }
        
        protected State GetState(string name) {
            foreach (var s in allStates) {
                if (s.ToString() == name) {
                    return s;
                }
            }

            return null;
        }

        public new bool IsStateActive(string name) {
            State s = rootMachine.currentState;
            while (s != null) {
                if (s.ToString() == name) {
                    return true;
                }

                if (s.subMachine != null) {
                    s = s.subMachine.currentState;
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
            
            return s.parentMachine.currentState == s || (s.parentMachine.currentState == null && s.parentMachine.defaultState == s);
        }

        public float TransitionLastTime(string state1, string state2) {
            State s1 = GetState(state1);

            foreach (var t in s1.transitions) {
                if (t.to.ToString() == state2) {
                    return t.lastTimeTakenUnscaled;
                }
            }

            return float.NegativeInfinity;
        }

        protected override void DoInput() {
            rootMachine.Update();

            var t = rootMachine.CheckTransitions();
            if (t != null) {
                t.notify?.Invoke();
                t.lastTimeTakenUnscaled = Time.unscaledTime;
                t.lastTimeTaken = Time.time;
                TransitionTo(t.to);
            }
        }

        protected void TransitionTo(State target) {
            Stack<State> t = new Stack<State>();

            while (target != null) {
                t.Push(target);

                target = target.parent;
            }

            rootMachine.TransitionTo(t);
        }

        protected Trigger trigger_OnDamage = new Trigger();
        protected virtual void OnDamage(Damage dmg) => trigger_OnDamage.Set();
    }
}