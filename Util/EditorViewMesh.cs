using UnityEngine;

namespace SBR {
    /// <summary>
    /// Can be used to disable renderers in game.
    /// </summary>
    public class EditorViewMesh : MonoBehaviour {
        /// <summary>
        /// Whether to hide the renderer in game.
        /// </summary>
        [Tooltip("Whether to hide the renderer in game.")]
        public bool hideInGame;

        private void Start() {
            if (hideInGame) {
                GetComponent<Renderer>().enabled = false;
            }
        }
    }

}