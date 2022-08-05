// The MIT License (MIT)
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