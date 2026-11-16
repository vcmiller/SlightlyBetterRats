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

using SBR.StateMachines;
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SBR.Editor {
    public class StateMachineEditorWindow : EditorWindow {
        private Operation op;
        private bool dirty = false;
        private float _sidePanelWidth;
        private bool resize = false;
        private readonly Color bgColor = new Color32(93, 93, 93, 255);
        private readonly Color lineColor = new Color32(70, 70, 70, 255);
        private readonly Color panelColor = new Color32(194, 194, 194, 255);
        private const float editorWindowTabHeight = 21.0f;
        
        private IStateMachine observing;

        [NonSerialized]
        private bool showSide = true;

        [NonSerialized]
        private string[] types;
        [NonSerialized]
        private string[] displayTypes;
        
        public static StateMachineDefinition def;

        [NonSerialized]
        private StateMachineDefinition.Transition lastSelectedTr;
        [NonSerialized]
        private StateMachineDefinition.State lastSelectedState;

        [NonSerialized]
        private StateMachineDefinition.State editingState;
        [NonSerialized]
        private Pair<StateMachineDefinition.State, StateMachineDefinition.Transition> editingTransition;

        public Vector2 scroll { get; private set; }
        public float zoom { get; private set; }

        public const float zoomMin = 0.5f;
        public const float zoomMax = 2f;

        private float sideWidth {
            get {
                if (Application.isPlaying) {
                    return 0;
                } else {
                    return _sidePanelWidth;
                }
            }
        }

        public StateMachineEditorWindow() {
            wantsMouseMove = true;
            editingState = null;
            editingTransition = new Pair<StateMachineDefinition.State, StateMachineDefinition.Transition>(null, null);
            zoom = 1.0f;
        }

        [MenuItem("Tools/Infohazard/Windows/State Machine Editor")]
        public static void ShowWindow() {
            GetWindow(typeof(StateMachineEditorWindow));
        }

        void UpdateSelection() {
            Repaint();
        }

        public Vector2 ToWorld(Vector2 screen) {
            return screen + scroll / zoom;
        }

        public Vector2 ToScreen(Vector2 world) {
            return world - scroll / zoom;
        }

        private void Update() {
            if (Application.isPlaying && def) {
                Repaint();
            }
        }

        void OnGUI() {
            foreach (Object obj in Selection.objects) {
                if (obj is StateMachineDefinition) {
                    def = (StateMachineDefinition)obj;
                    break;
                }
            }

            if (!Application.isPlaying) {
                ShowSidebar();
            }

            Rect viewportRect = new Rect(0, 0, position.width - sideWidth, position.height);
            EditorGUI.DrawRect(viewportRect, bgColor);

            GUI.EndGroup();
            
            Rect clippedArea = new Rect(0, 0, position.width - sideWidth, position.height);
            clippedArea.size /= zoom;
            clippedArea.y += editorWindowTabHeight / zoom;
            GUI.BeginGroup(clippedArea);

            bool repaint = false;

            if (def != null) {
                Event cur = Event.current;
                Matrix4x4 gm = GUI.matrix;
                GUI.matrix = Matrix4x4.Scale(new Vector3(zoom, zoom, 1));

                UpdateView(ref repaint);

                if (!Application.isPlaying) {
                    if (op != null) {
                        if (def != op.definition) {
                            op.Cancel();
                            op = null;
                        } else {
                            op.Update();

                            if (op.done) {
                                op = null;
                                repaint = true;
                                EditorUtility.SetDirty(def);
                                dirty = true;
                            }
                        }
                    } else {
                        StateMachineDefinition.State selected = def.SelectState(ToWorld(cur.mousePosition));
                        Pair<StateMachineDefinition.State, StateMachineDefinition.Transition> selectedTr = def.SelectTransition(ToWorld(cur.mousePosition));

                        if (selectedTr.Item1 == null) {
                            if (selected != lastSelectedState) {
                                repaint = true;
                                lastSelectedState = selected;
                            }

                            if (lastSelectedTr != null) {
                                lastSelectedTr = null;
                                repaint = true;
                            }
                        } else {
                            if (selectedTr.Item2 != lastSelectedTr) {
                                repaint = true;
                                lastSelectedTr = selectedTr.Item2;
                            }

                            if (lastSelectedState != null) {
                                lastSelectedState = null;
                                repaint = true;
                            }
                        }

                        if (cur.type == EventType.MouseDown) {
                            if (cur.button == 0) {
                                if (selectedTr.Item1 != null) {
                                    editingTransition = selectedTr;
                                    editingState = null;
                                    repaint = true;
                                } else if (selected != null) {
                                    editingState = selected;
                                    editingTransition = new Pair<StateMachineDefinition.State, StateMachineDefinition.Transition>(null, null);
                                    repaint = true;

                                    if (cur.clickCount == 1) {
                                        op = new MoveStateOperation(def, this, selected);
                                    } else if (cur.clickCount == 2) {
                                        op = new RenameStateOperation(def, this, selected);
                                    }
                                } else {
                                    editingState = null;
                                    editingTransition = new Pair<StateMachineDefinition.State, StateMachineDefinition.Transition>(null, null);
                                    repaint = true;
                                }
                            } else if (cur.button == 1) {
                                if (selected == null && lastSelectedTr == null) {
                                    GenericMenu menu = new GenericMenu();

                                    menu.AddItem(new GUIContent("Create State"), false, () => {
                                        Undo.RecordObject(def, "Create State");
                                        StateMachineDefinition.State s = def.AddState();
                                        s._position = ToWorld((cur.mousePosition - Vector2.up * editorWindowTabHeight) / zoom);
                                        MoveStateOperation.Snap(ref s._position);
                                        op = new RenameStateOperation(def, this, s);
                                        EditorUtility.SetDirty(def);
                                        dirty = true;
                                    });

                                    GUI.EndGroup();
                                    menu.ShowAsContext();
                                    GUI.BeginGroup(clippedArea);
                                } else if (selectedTr.Item1 != null) {
                                    GenericMenu menu = new GenericMenu();

                                    menu.AddItem(new GUIContent("Remove Transition"), false, () => {
                                        Undo.RecordObject(def, "Remove Transition");
                                        selectedTr.Item1.RemoveTransition(selectedTr.Item2);
                                        EditorUtility.SetDirty(def);
                                        dirty = true;
                                        repaint = true;
                                    });

                                    GUI.EndGroup();
                                    menu.ShowAsContext();
                                    GUI.BeginGroup(clippedArea);
                                } else if (selected != null) {
                                    GenericMenu menu = new GenericMenu();

                                    menu.AddItem(new GUIContent("Delete State"), false, () => {
                                        Undo.RecordObject(def, "Delete State");
                                        def.RemoveState(selected);
                                        EditorUtility.SetDirty(def);
                                        dirty = true;
                                        repaint = true;
                                    });

                                    menu.AddItem(new GUIContent("Rename State"), false, () => {
                                        op = new RenameStateOperation(def, this, selected);
                                    });

                                    menu.AddItem(new GUIContent("Add Transition"), false, () => {
                                        op = new MakeTransitionOperation(def, this, selected);
                                    });

                                    StateMachineDefinition.State parent = def.GetState(selected._parent);

                                    if (parent == null) {
                                        if (selected._name != def._defaultState) {
                                            menu.AddItem(new GUIContent("Make Default State"), false, () => {
                                                Undo.RecordObject(def, "Set Default State");
                                                def._defaultState = selected._name;
                                                dirty = true;
                                                repaint = true;
                                            });
                                        }
                                    } else {
                                        if (selected._name != parent._localDefault) {
                                            menu.AddItem(new GUIContent("Make Local Default"), false, () => {
                                                Undo.RecordObject(def, "Set Local Default");
                                                parent._localDefault = selected._name;
                                                dirty = true;
                                                repaint = true;
                                            });
                                        }
                                    }

                                    if (selected._hasChildren) {
                                        menu.AddItem(new GUIContent("Remove Sub-Machine"), false, () => {
                                            Undo.RecordObject(def, "Remove Sub-Machine");
                                            def.RemoveSub(selected, MoveStateOperation.snap * 2);
                                        });

                                        menu.AddItem(new GUIContent("Create Child State"), false, () => {
                                            Undo.RecordObject(def, "Create Child State");
                                            StateMachineDefinition.State s = def.AddState();
                                            s._position = ToWorld((cur.mousePosition - Vector2.up * editorWindowTabHeight) / zoom);
                                            def.SetStateParent(s, selected, MoveStateOperation.snap);
                                            MoveStateOperation.Snap(ref s._position);
                                            op = new RenameStateOperation(def, this, s);
                                            EditorUtility.SetDirty(def);
                                            dirty = true;
                                        });
                                    } else {
                                        menu.AddItem(new GUIContent("Make Sub-Machine"), false, () => {
                                            Undo.RecordObject(def, "Make Sub-Machine");
                                            def.CreateSub(selected, MoveStateOperation.snap * 2);
                                        });
                                    }

                                    GUI.EndGroup();
                                    menu.ShowAsContext();
                                    GUI.BeginGroup(clippedArea);
                                }
                            }
                        } else if (cur.type == EventType.KeyDown) {
                            if (cur.keyCode == KeyCode.Delete) {
                                if (selected != null) {
                                    Undo.RecordObject(def, "Delete State");
                                    def.RemoveState(selected);
                                    EditorUtility.SetDirty(def);
                                    dirty = true;
                                    repaint = true;
                                } else if (selectedTr.Item1 != null) {
                                    Undo.RecordObject(def, "Remove Transition");
                                    selectedTr.Item1.RemoveTransition(selectedTr.Item2);
                                    EditorUtility.SetDirty(def);
                                    dirty = true;
                                    repaint = true;
                                }
                            }
                        } else if (cur.type == EventType.ValidateCommand) {
                            editingState = selected = lastSelectedState = null;
                            editingTransition = selectedTr = new Pair<StateMachineDefinition.State, StateMachineDefinition.Transition>();
                            lastSelectedTr = null;
                            dirty = true;
                            repaint = true;
                        }
                    }
                }

                if (Event.current.type != EventType.Repaint && (repaint || (op != null && op.repaint))) {
                    Repaint();
                }

                Handles.BeginGUI();

                Handles.color = lineColor;
                Vector2 sz = scroll / zoom;

                for (float x = -sz.x % MoveStateOperation.snap; x < clippedArea.width; x += MoveStateOperation.snap) {
                    Handles.DrawLine(new Vector3(x, 0), new Vector3(x, clippedArea.height));
                }

                for (float y = -sz.y % MoveStateOperation.snap; y < clippedArea.height; y += MoveStateOperation.snap) {
                    Handles.DrawLine(new Vector3(0, y), new Vector3(clippedArea.width, y));
                }

                if (Application.isPlaying) {
                    Object observingObject = observing as UnityEngine.Object;
                    if (!observingObject || observing.GameObject != Selection.activeGameObject) {
                        if (Selection.activeGameObject) {
                            observing = Selection.activeGameObject.GetComponent<IStateMachine>();
                        }
                    }
                } else if (!Application.isPlaying) {
                    observing = null;
                }
                Color oldColor = GUI.color;

                if (def._states != null) {
                    EditorGUI.BeginDisabledGroup(op != null && op is RenameStateOperation);
                    foreach (StateMachineDefinition.State state in def._states) {
                        if (op == null || op.state != state || op.showBaseGUI) {
                            string s = state._name;
                            StateMachineDefinition.State parent = def.GetState(state._parent);
                            if (parent == null && def._defaultState == s) {
                                s += "\n<default state>";
                            } else if (parent != null && parent._localDefault == s) {
                                s += "\n<local default>";
                            }

                            Rect rect = state.Rect;
                            rect.position = ToScreen(rect.position);

                            GUI.SetNextControlName("StateButton");
                            
                            if (observing != null && cur.type == EventType.Repaint) {
                                if (observing.IsStateActive(state._name)) {
                                    GUI.color = new Color(0.5f, 0.7f, 1.0f);
                                } else if (observing.IsStateRemembered(state._name)) {
                                    GUI.color = new Color(0.9f, 0.8f, 1.0f);
                                } else {
                                    GUI.color = oldColor;
                                }
                            }

                            if (!state._hasChildren) {
                                if (state != editingState) {
                                    GUI.Button(rect, s);
                                } else {
                                    GUI.Button(rect, "");

                                    GUIStyle style = new GUIStyle(GUI.skin.label);
                                    style.alignment = TextAnchor.MiddleCenter;
                                    style.normal.textColor = Color.blue;
                                    style.fontStyle = FontStyle.Bold;

                                    GUI.Label(rect, s, style);
                                }
                            } else {
                                GUI.Button(rect, "");

                                GUIStyle style = new GUIStyle(GUI.skin.label);
                                style.alignment = TextAnchor.UpperCenter;

                                if (state == editingState) {
                                    style.normal.textColor = Color.blue;
                                    style.fontStyle = FontStyle.Bold;
                                }

                                GUI.Label(rect, s, style);

                                Rect innerRect = rect;

                                innerRect.xMin += 4;
                                innerRect.xMax -= 4;

                                innerRect.yMin += 32;
                                innerRect.yMax -= 4;

                                GUI.color = oldColor;
                                EditorGUI.DrawRect(innerRect, bgColor);

                                Handles.color = lineColor;
                                for (float x = rect.xMin + MoveStateOperation.snap; x < rect.xMax; x += MoveStateOperation.snap) {
                                    Handles.DrawLine(new Vector3(x, innerRect.yMin), new Vector3(x, innerRect.yMax));
                                }

                                for (float y = rect.yMin + MoveStateOperation.snap * 2; y < rect.yMax; y += MoveStateOperation.snap) {
                                    Handles.DrawLine(new Vector3(innerRect.xMin, y), new Vector3(innerRect.xMax, y));
                                }
                            }
                        }
                    }
                    EditorGUI.EndDisabledGroup();

                    if (op != null) {
                        op.OnGUI();
                    }

                    foreach (StateMachineDefinition.State from in def._states) {
                        if (from._transitions != null) {
                            foreach (StateMachineDefinition.Transition tr in from._transitions) {
                                if (Application.isPlaying) {
                                    Handles.color = Color.black;
                                    if (observing != null) {
                                        float t = Time.unscaledTime - observing.TransitionLastTime(from._name, tr._to);
                                        if (t < 0.5f) {
                                            Handles.color = Color.Lerp(Color.black, Color.green, 1.0f - t * 2);
                                        }
                                    }
                                } else {
                                    if (tr != lastSelectedTr && tr != editingTransition.Item2) {
                                        Handles.color = Color.black;
                                    } else {
                                        Handles.color = Color.blue;
                                    }
                                }

                                if (def.GetState(tr._to) != null) {
                                    Pair<Vector2, Vector2> line = def.GetTransitionPoints(from, tr);
                                    Vector2 src = ToScreen(line.Item1);
                                    Vector2 dest = ToScreen(line.Item2);

                                    Vector2 v = (dest - src).normalized;
                                    Vector2 ortho = new Vector2(v.y, -v.x);

                                    Vector2 arrow = ortho - v;
                                    Vector2 mid = (src + dest) / 2;

                                    Handles.DrawAAPolyLine(3, src, dest);
                                    Handles.DrawAAPolyLine(3, mid + v * 5, mid + arrow * 6);
                                }
                            }
                        }
                    }
                }
                
                Handles.EndGUI();

                GUI.matrix = gm;
            } else if (op != null) {
                op.Cancel();
                op = null;
            }
        }

        private void UpdateView(ref bool repaint) {
            Rect viewportRect = new Rect(0, 0, position.width - sideWidth, position.height);
            Event cur = Event.current;
            if (cur.type == EventType.MouseDrag && cur.button == 2) {
                scroll -= cur.delta * zoom;
                repaint = true;
            } else if (cur.type == EventType.KeyDown && cur.keyCode == KeyCode.F && !(op is RenameStateOperation)) {
                StateMachineDefinition.State state = def.GetState(def._defaultState);
                if (state == null && def._states.Count > 0) {
                    state = def._states[0];
                }

                if (state != null) {
                    scroll = state._position - viewportRect.size / 2 + state._size / 2;
                    repaint = true;
                    zoom = 1;
                } else {
                    scroll = Vector2.zero;
                    repaint = true;
                    zoom = 1;
                }
            } else if (cur.isScrollWheel) {
                Vector2 mw = ToWorld(cur.mousePosition);
                Vector2 swo = scroll / zoom;
                Vector2 wo = swo - mw;

                float oz = zoom;
                zoom *= Mathf.Pow(1.1f, -cur.delta.y);
                zoom = Mathf.Clamp(zoom, zoomMin, zoomMax);

                wo *= (oz / zoom);

                scroll = (wo + mw) * zoom;

                repaint = true;
            }
        }

        private void ShowSidebar() {
            ResizeScrollView();

            if (def && showSide) {
                float padding = 20;
                EditorGUI.DrawRect(new Rect(position.width - _sidePanelWidth, 0, _sidePanelWidth, position.height), panelColor);
                GUILayout.BeginArea(new Rect(position.width - _sidePanelWidth + padding, padding, _sidePanelWidth - padding * 2, position.height - padding * 2));
                EditorGUILayout.BeginVertical();
                float w = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 1;

                if (GUILayout.Button(dirty ? "Save *" : "Save")) {
                    AssetDatabase.SaveAssets();
                    dirty = false;
                    StateMachineClassGenerator.GenerateAbstractClass(def);
                }

                if (GUILayout.Button("New Impl Class...")) {
                    string path = EditorUtility.SaveFilePanelInProject("Save new class", def.name + "Impl.cs", "cs", "Enter a name for the new impl class");
                    if (path.Length > 0) {
                        StateMachineClassGenerator.GenerateImplClass(def, path);
                    }
                }

                if (types == null) {
                    types = typeof(IStateMachine).Assembly.GetTypes()
                        .Where(p => !p.IsInterface && typeof(IStateMachine).IsAssignableFrom(p))
                        .Select(t => t.FullName).ToArray();
                }

                if (displayTypes == null) {
                    displayTypes = types.Select(s => {
                        int i = s.LastIndexOf('.');
                        if (i >= 0 && i < s.Length - 1) {
                            s = s.Substring(i + 1);
                        }
                        return s;
                    }).ToArray();
                }

                int index = Array.IndexOf(types, def._baseClass);
                if (index < 0) {
                    index = Array.IndexOf(types, typeof(StateMachine<>).FullName);
                }
                int prev = index;

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Base Class");
                index = EditorGUILayout.Popup(index, displayTypes);

                if (prev != index) {
                    dirty = true;
                }

                EditorGUILayout.EndHorizontal();
                def._baseClass = types[index];

                if (editingState != null) {
                    EditorGUILayout.LabelField("State " + editingState._name);
                    EditorGUILayout.LabelField("State Events", EditorStyles.boldLabel);

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Enter: ");
                    bool enter = EditorGUILayout.Toggle(editingState._hasEnter);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("During: ");
                    bool during = EditorGUILayout.Toggle(editingState._hasDuring);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Exit: ");
                    bool exit = EditorGUILayout.Toggle(editingState._hasExit);
                    EditorGUILayout.EndHorizontal();


                    if (enter != editingState._hasEnter || during != editingState._hasDuring || exit != editingState._hasExit) {
                        Undo.RecordObject(def, "Edit State");
                        editingState._hasEnter = enter;
                        editingState._hasDuring = during;
                        editingState._hasExit = exit;

                        dirty = true;
                        EditorUtility.SetDirty(def);
                    }

                } else if (editingTransition.Item1 != null) {
                    EditorGUILayout.LabelField("Transition From " + editingTransition.Item1._name + " To " + editingTransition.Item2._to);
                    EditorGUILayout.LabelField("Transition Events", EditorStyles.boldLabel);

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Notify: ");
                    bool notify = EditorGUILayout.Toggle("Notify: ", editingTransition.Item2._hasNotify);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.LabelField("Conditions", EditorStyles.boldLabel);

                    float cooldown = editingTransition.Item2._cooldown;
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Cooldown:");
                    cooldown = EditorGUILayout.FloatField(cooldown);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Mode: ");
                    StateMachineDefinition.TransitionMode mode = (StateMachineDefinition.TransitionMode)EditorGUILayout.EnumPopup(editingTransition.Item2._mode);
                    EditorGUILayout.EndHorizontal();

                    float exitTime = editingTransition.Item2._exitTime;
                    string message = editingTransition.Item2._message;
                    if (editingTransition.Item2._mode == StateMachineDefinition.TransitionMode.Time) {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Exit Time: ");
                        exitTime = EditorGUILayout.FloatField(exitTime);
                        EditorGUILayout.EndHorizontal();
                    } else if (editingTransition.Item2._mode == StateMachineDefinition.TransitionMode.Message) {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Message: ");
                        message = EditorGUILayout.TextField(editingTransition.Item2._message);
                        EditorGUILayout.EndHorizontal();
                    }

                    if (notify != editingTransition.Item2._hasNotify ||
                        exitTime != editingTransition.Item2._exitTime ||
                        message != editingTransition.Item2._message ||
                        cooldown != editingTransition.Item2._cooldown ||
                        mode != editingTransition.Item2._mode) {
                        Undo.RecordObject(def, "Edit Transition");
                        editingTransition.Item2._hasNotify = notify;
                        editingTransition.Item2._exitTime = exitTime;
                        editingTransition.Item2._cooldown = cooldown;
                        editingTransition.Item2._mode = mode;
                        editingTransition.Item2._message = message;
                        def.OnValidate();
                        dirty = true;
                        EditorUtility.SetDirty(def);
                    }
                }

                EditorGUILayout.EndVertical();
                EditorGUIUtility.labelWidth = w;

                GUILayout.EndArea();
            }
        }

        private void OnEnable() {
            _sidePanelWidth = 200;

            Selection.selectionChanged += UpdateSelection;
            titleContent = new GUIContent("SM Editor", Resources.Load<Texture2D>("StateMachine"));
        }

        private void OnDisable() {
            Selection.selectionChanged -= UpdateSelection;
        }

        private void ResizeScrollView() {
            Rect cursorChangeRect = new Rect(position.width - _sidePanelWidth - 5.0f, 0, 10f, position.height);

            EditorGUIUtility.AddCursorRect(cursorChangeRect, MouseCursor.ResizeHorizontal);

            if (Event.current.type == EventType.MouseDown && cursorChangeRect.Contains(Event.current.mousePosition)) {
                resize = true;
            }
            if (resize) {
                _sidePanelWidth = position.width - Event.current.mousePosition.x;
                Repaint();
            }
            if (Event.current.type == EventType.MouseUp)
                resize = false;
        }
    }
}
