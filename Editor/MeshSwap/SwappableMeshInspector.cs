using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace SBR.Editor {
    // This is causing too many problems. Might get back to it another time.
    //[CustomPreview(typeof(SwappableMesh))]
    public class SwappableMeshPreview : ObjectPreview {
        private GameObject gameObjectTarget => (target as Component).gameObject;
        private (Material[] mats, Mesh mesh, Matrix4x4 transform, Bounds bounds)[] _targetMeshes;

        private PreviewRenderUtility _previewRenderUtility;

        private Vector2 _drag;
        private Vector3 _cameraOrigin;

        //Fetch all the relevent information
        private void ValidateData() {
            if (_previewRenderUtility == null) {
                _previewRenderUtility = new PreviewRenderUtility();

                //We set the previews camera to 6 units back, look towards the middle of the 'scene'
                _previewRenderUtility.camera.transform.position = new Vector3(0, 0, -6);
                _previewRenderUtility.camera.transform.rotation = Quaternion.identity;
            }
        }

        public override void Initialize(Object[] targets) {
            base.Initialize(targets);

            _targetMeshes = gameObjectTarget.GetComponentsInChildren<MeshRenderer>()
                    .Select(mr => (mr, mf: mr.GetComponent<MeshFilter>()))
                    .Where(t => t.mf)
                    .Select(pair => (pair.mr.sharedMaterials, pair.mf.sharedMesh, pair.mr.transform.localToWorldMatrix, pair.mr.bounds))

                    .Concat(gameObjectTarget.GetComponentsInChildren<SkinnedMeshRenderer>()
                    .Select(smr => (smr.sharedMaterials, smr.sharedMesh, smr.localToWorldMatrix, smr.bounds)))

                    .Concat(gameObjectTarget.GetComponentsInChildren<SwappableMesh>()
                    .SelectMany(swp => swp.PrefabMeshes)
                    .Select(mesh => (mesh.mats, mesh.mesh, mesh.transform, mesh.bounds)))

                    .ToArray();
        }

        public override bool HasPreviewGUI() {
            //Validate data - this is always called before OnPreviewGUI
            ValidateData();

            return true;
        }

        public override void OnPreviewGUI(Rect r, GUIStyle background) {
            _drag = Drag2D(_drag, r);

            //Only render our 3D 'preview' when the UI is 'repainting'.
            //The OnPreviewGUI, like other GUI methods, will be called LOTS
            //of times ever frame to handle different events.
            //We only need to Render our preview once when the GUI is being repainted!
            if (Event.current.type == EventType.Repaint) {
                //Tell the PRU to prepair itself - we pass along the
                //rect of the preview area so the PRU knows what size 
                //of a preview to render.
                _previewRenderUtility.BeginPreview(r, background);

                //We draw our mesh manually - it is not attached to any 'gameobject' in the preview 'scene'.
                //The preview 'scene' only contains a camera and a light. We need to render things manually.
                //We pass along the mesh set on the mesh filter and the material set on the renderer
                foreach ((Material[] mats, Mesh mesh, Matrix4x4 transform, Bounds bounds) in _targetMeshes) {
                    for (int i = 0; i < mesh.subMeshCount && i < mats.Length; i++) {
                        _previewRenderUtility.DrawMesh(mesh, transform, mats[i], i);
                    }
                }

                //Tell the camera to actually render the preview.
                _previewRenderUtility.camera.transform.position = Vector2.zero;
                _previewRenderUtility.camera.transform.rotation = Quaternion.Euler(new Vector3(-_drag.y, -_drag.x, 0));
                _previewRenderUtility.camera.transform.position = _previewRenderUtility.camera.transform.forward * -6f;
                _previewRenderUtility.camera.Render();

                //Now that we are done, we can end the preview. This method will spit out a Texture
                //The texture contains the image that was rendered by the preview utillity camera :)
                Texture resultRender = _previewRenderUtility.EndPreview();

                //If we omit the line bellow, then you wouldnt actually see anything in the preview!
                //The preview image is generated, but that was all done in our 'virtual' PreviewRenderUtility 'scene'.
                //We still need to draw something in the PreviewGUI area..!

                //So we draw the image that was generated into the preview GUI area, filling the entire area with this image.
                GUI.DrawTexture(r, resultRender, ScaleMode.StretchToFill, false);
            }
        }
        public static Vector2 Drag2D(Vector2 scrollPosition, Rect position) {
            int controlID = GUIUtility.GetControlID("Slider".GetHashCode(), FocusType.Passive);
            Event current = Event.current;
            switch (current.GetTypeForControl(controlID)) {
                case EventType.MouseDown:
                    if (position.Contains(current.mousePosition) && position.width > 50f) {
                        GUIUtility.hotControl = controlID;
                        current.Use();
                        EditorGUIUtility.SetWantsMouseJumping(1);
                    }
                    break;
                case EventType.MouseUp:
                    if (GUIUtility.hotControl == controlID) {
                        GUIUtility.hotControl = 0;
                    }
                    EditorGUIUtility.SetWantsMouseJumping(0);
                    break;
                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == controlID) {
                        scrollPosition -= current.delta * (float)((!current.shift) ? 1 : 3) / Mathf.Min(position.width, position.height) * 140f;
                        scrollPosition.y = Mathf.Clamp(scrollPosition.y, -90f, 90f);
                        current.Use();
                        GUI.changed = true;
                    }
                    break;
            }
            return scrollPosition;
        }

        void OnDisable() {
            //Gotta clean up after yourself!
            Debug.Log("HI");
            _previewRenderUtility.Cleanup();
            _previewRenderUtility = null;
        }
    }
}