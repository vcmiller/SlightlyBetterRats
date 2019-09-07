using SBR.StateMachines;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SBR.Editor {
    public static class StateMachineClassGenerator {
        private static readonly string implClassTemplate = @"using UnityEngine;
using SBR;
using System.Collections.Generic;

public class {0} {{
{1}
}}
";

        private static readonly string abstractClassTemplate = @"using SBR;
using SBR.StateMachines;
using System.Collections.Generic;

#pragma warning disable 649
public abstract class {5} {{
    public enum StateID {{
        {1}
    }}

    private class State : SBR.StateMachines.State {{
        public StateID id;

        public override string ToString() {{
            return id.ToString();
        }}
    }}

    public {0}() {{
{2}
{3}
    }}

    public StateID state {{
        get {{
            State st = rootMachine.activeLeaf as State;
            return st.id;
        }}

        set {{
            stateName = value.ToString();
        }}
    }}

{4}
{6}
}}
";

        private static readonly string nl = @"
";

        public static void GenerateImplClass(StateMachineDefinition def, string path) {
            string newClassName = Path.GetFileNameWithoutExtension(path);
            string abstractClassName = def.name;
            Type baseType = GetType(def.baseClass);
            if (baseType.ContainsGenericParameters) {
                abstractClassName += "`1";
            }
            
            string generated = string.Format(implClassTemplate,
                GetClassDeclaration(abstractClassName, newClassName), 
                GetFunctionDeclarations(def, true));

            StreamWriter outStream = new StreamWriter(path);
            outStream.Write(generated);
            outStream.Close();
            AssetDatabase.Refresh();
        }

        public static void GenerateAbstractClass(StateMachineDefinition def) {
            string generated = string.Format(abstractClassTemplate, 
                def.name, 
                GetStateEnums(def), 
                GetStateInitializers(def), 
                GetTransitionInitializers(def), 
                GetFunctionDeclarations(def), 
                GetClassDeclaration(def.baseClass, def.name),
                GetTriggerDeclarations(def));

            string defPath = AssetDatabase.GetAssetPath(def);

            if (defPath.Length > 0) {
                string newPath = defPath.Substring(0, defPath.LastIndexOf(".")) + ".cs";

                StreamWriter outStream = new StreamWriter(newPath);
                outStream.Write(generated);
                outStream.Close();
                AssetDatabase.Refresh();
            }
        }

        private static Type GetType(string typeName) {
            return typeof(StateMachine<>).Assembly.GetType(typeName);
        }

        private static string GetClassDeclaration(string baseClass, string className) {
            Type type = GetType(baseClass);
            if (type.ContainsGenericParameters) {
                var args = type.GetGenericArguments();
                if (args.Length > 1) {
                    throw new UnityException("StateMachine base class may only have one type parameter.");
                } else {
                    className += "<T>";
                    baseClass = baseClass.Substring(0, baseClass.IndexOf('`'));

                    var param = args[0];
                    var constraints = param.GetGenericParameterConstraints();
                    if (constraints.Length > 0) {
                        baseClass += "<T> where T : " + string.Join(", ", constraints.Select(c => c.FullName)) + ", new()";
                    }
                }
            }

            return className + " : " + baseClass;
        }

        private static string GetStateEnums(StateMachineDefinition def) {
            string str = "";

            for (int i = 0; i < def.states.Count; i++) {
                str += def.states[i].name;

                if (i < def.states.Count - 1) {
                    str += ", ";
                }
            }

            return str;
        }

        public static string GetStateInitializers(StateMachineDefinition def) {
            string str = "        allStates = new State[" + def.states.Count + "];" + nl + nl;
            for (int i = 0; i < def.states.Count; i++) {
                str += GetStateInitializer(def, i);
            }

            if (def.defaultState != null && def.defaultState.Length > 0 && def.GetState(def.defaultState) != null) {
                str += "        rootMachine.defaultState = state" + def.defaultState + ";" + nl;
            } else {
                str += "        rootMachine.defaultState = allStates[0];" + nl;
            }

            for (int i = 0; i < def.states.Count; i++) {
                str += GetStateParentChildInitializer(def, i);
            }

            return str;
        }

        public static string GetStateInitializer(StateMachineDefinition def, int index) {
            var state = def.states[index];
            string variable = "state" + state.name;

            string str = "        State " + variable + " = new State() {" + nl;
            str += "            id = StateID." + state.name + "," + nl;

            if (state.hasEnter) {
                str += "            enter = StateEnter_" + state.name + "," + nl;
            }

            if (state.hasDuring) {
                str += "            during = State_" + state.name + "," + nl;
            }

            if (state.hasExit) {
                str += "            exit = StateExit_" + state.name + "," + nl;
            }

            if (state.hasChildren && def.GetChildren(state.name).Count > 0) {
                str += "            subMachine = new SubStateMachine()," + nl;
            }

            str += "            transitions = new List<Transition>(" + (state.transitions == null ? 0 : state.transitions.Count) + ")" + nl;
            str += "        };" + nl;
            str += "        allStates[" + index + "] = " + variable + ";" + nl;

            str += nl;
            return str;
        }

        public static string GetStateParentChildInitializer(StateMachineDefinition def, int index) {
            string str = "";
            var state = def.states[index];
            string p = state.parent;

            if (p != null && p.Length > 0) {
                var parent = def.GetState(p);
                if (parent != null && parent.hasChildren) {
                    str += "        state" + state.name + ".parent = state" + p + ";" + nl;
                    str += "        state" + state.name + ".parentMachine = state" + p + ".subMachine;" + nl;
                } else {
                    Debug.LogWarning("State " + state.name + " has non-existant parent " + p + ".");
                    str += "        state" + state.name + ".parentMachine = rootMachine;" + nl;
                }
            } else {
                str += "        state" + state.name + ".parentMachine = rootMachine;" + nl;
            }

            if (state.hasChildren) {
                var children = def.GetChildren(state.name);

                if (children.Count > 0) {
                    StateMachineDefinition.State defState = null;
                    foreach (var child in children) {
                        if (child.name == state.localDefault) {
                            defState = child;
                            break;
                        }
                    }

                    if (defState == null) {
                        defState = children[0];
                    }

                    str += "        state" + state.name + ".subMachine.defaultState = state" + defState.name + ";" + nl;
                }
            }

            return str;
        }

        public static string GetTransitionInitializers(StateMachineDefinition def) {
            string str = "";
            foreach (var state in def.states) {
                if (state.transitions != null) {
                    for (int i = 0; i < state.transitions.Count; i++) {
                        var t = state.transitions[i];
                        var to = def.GetState(t.to);
                        if (to == null) {
                            Debug.LogWarning("Ignoring transition from " + state.name + " to non-existant state " + t.to + ".");
                        } else if (def.IsAncestor(state, to) || def.IsAncestor(to, state)) {
                            Debug.LogWarning("Ignoring transition from " + state.name + " to " + t.to + " because one state is a direct ancestor.");
                        } else {
                            str += GetTransitionInitializer(state, t, i);
                        }
                    }
                }
            }
            return str;
        }

        public static string GetTransitionInitializer(StateMachineDefinition.State from, StateMachineDefinition.Transition to, int index) {
            string variable = "transition" + from.name + to.to;

            string str = "        Transition " + variable + " = new Transition() {" + nl;
            str += "            from = state" + from.name + "," + nl;
            str += "            to = state" + to.to + "," + nl;
            str += "            exitTime = " + to.exitTime + "f," + nl;
            str += "            cooldown = " + to.cooldown + "f," + nl;
            str += "            mode = StateMachineDefinition.TransitionMode." + to.mode.ToString() + "," + nl;

            if (to.hasNotify) {
                str += "            notify = TransitionNotify_" + from.name + "_" + to.to + "," + nl;
            }

            if (to.mode == StateMachineDefinition.TransitionMode.Condition) {
                str += "            cond = TransitionCond_" + from.name + "_" + to.to + "," + nl;
            } else if (to.mode == StateMachineDefinition.TransitionMode.Message) {
                str += "            cond = trigger_" + to.message + ".Get," + nl;
            } else if (to.mode == StateMachineDefinition.TransitionMode.Damage) {
                str += "            cond = trigger_OnDamage.Get," + nl;
            }

            str += "        };" + nl;
            str += "        state" + from.name + ".transitions.Add(" + variable + ");" + nl;
            str += nl;
            return str;
        }

        public static string GetTriggerDeclarations(StateMachineDefinition def) {
            string str = "";
            HashSet<string> created = new HashSet<string>();
            foreach (var state in def.states) {
                foreach (var tr in state.transitions) {
                    if (tr.mode == StateMachineDefinition.TransitionMode.Message && created.Add(tr.message)) {
                        str += "    protected readonly Trigger trigger_" + tr.message + " = new Trigger();" + nl;
                        str += "    public void " + tr.message + "() => trigger_" + tr.message + ".Set();" + nl;
                    }
                }
            }
            return str;
        }

        public static string GetFunctionDeclarations(StateMachineDefinition def, bool impl = false) {
            string vo = impl ? "override" : "abstract";
            string end = impl ? "() { }" + nl : "();" + nl;
            string end2 = impl ? "() { return false; }" + nl : "();" + nl;

            string str = "";
            foreach (var state in def.states) {
                if (state.hasEnter) {
                    str += "    protected " + vo + " void StateEnter_" + state.name + end;
                }

                if (state.hasDuring) {
                    str += "    protected " + vo + " void State_" + state.name + end;
                }

                if (state.hasExit) {
                    str += "    protected " + vo + " void StateExit_" + state.name + end;
                }
            }

            str += "" + nl;

            foreach (var state in def.states) {
                if (state.transitions != null) {
                    foreach (var trans in state.transitions) {
                        if (trans.mode == StateMachineDefinition.TransitionMode.Condition) {
                            str += "    protected " + vo + " bool TransitionCond_" + state.name + "_" + trans.to + end2;
                        }

                        if (trans.hasNotify) {
                            str += "    protected " + vo + " void TransitionNotify_" + state.name + "_" + trans.to + end;
                        }
                    }
                }
            }

            return str;
        }
    }
}