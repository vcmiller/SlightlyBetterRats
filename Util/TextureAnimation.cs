using UnityEngine;

namespace SBR {
    /// <summary>
    /// Used for animating the scale, offset, and texture of a material.
    /// </summary>
    public class TextureAnimation : MonoBehaviour {
        /// <summary>
        /// Rate at which the texture's scale changes.
        /// </summary>
        [Tooltip("Rate at which the texture's scale changes.")]
        public Vector2 scaleRate;

        /// <summary>
        /// Rate at which the texture's offset changes.
        /// </summary>
        [Tooltip("Rate at which the texture's offset changes.")]
        public Vector2 offsetRate;

        /// <summary>
        /// Textures to cycle through. Can be empty for no frame animation.
        /// </summary>
        [Tooltip("Textures to cycle through. Can be empty for no frame animation.")]
        public Texture2D[] textures;

        /// <summary>
        /// Framerate at which to cycle textures.
        /// </summary>
        [Tooltip("Framerate at which to cycle textures.")]
        public int framerate = 10;

        private Material material;
        private int curTexture = 0;
        private CooldownTimer changeTimer;

        private bool HasTextureAnimation() {
            return framerate > 0 && textures != null && textures.Length > 0;
        }
        
        void Start() {
            material = GetComponent<MeshRenderer>().material;
            if (HasTextureAnimation()) {
                changeTimer = new CooldownTimer(1.0f / framerate);
            }
        }
        
        void Update() {
            material.mainTextureOffset += offsetRate * Time.deltaTime;
            material.mainTextureScale += scaleRate * Time.deltaTime;

            if (changeTimer != null && changeTimer.Use()) {
                curTexture = (curTexture + 1) % textures.Length;
                material.mainTexture = textures[curTexture];
                material.SetTexture("_EmissionMap", textures[curTexture]);
            }
        }
    }

}