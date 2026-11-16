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

using UnityEngine;
using UnityEngine.Serialization;

namespace SBR {
    /// <summary>
    /// Used for animating the scale, offset, and texture of a material.
    /// </summary>
    public class TextureAnimation : MonoBehaviour {
        /// <summary>
        /// Rate at which the texture's scale changes.
        /// </summary>
        [FormerlySerializedAs("scaleRate"),Tooltip("Rate at which the texture's scale changes.")]
        public Vector2 _scaleRate;

        /// <summary>
        /// Rate at which the texture's offset changes.
        /// </summary>
        [FormerlySerializedAs("offsetRate"),Tooltip("Rate at which the texture's offset changes.")]
        public Vector2 _offsetRate;

        /// <summary>
        /// Textures to cycle through. Can be empty for no frame animation.
        /// </summary>
        [FormerlySerializedAs("textures"),Tooltip("Textures to cycle through. Can be empty for no frame animation.")]
        public Texture2D[] _textures;

        /// <summary>
        /// Framerate at which to cycle textures.
        /// </summary>
        [FormerlySerializedAs("framerate"),Tooltip("Framerate at which to cycle textures.")]
        public int _framerate = 10;

        private Material _material;
        private int _curTexture = 0;
        private CooldownTimer _changeTimer;

        private bool HasTextureAnimation() {
            return _framerate > 0 && _textures != null && _textures.Length > 0;
        }
        
        void Start() {
            _material = GetComponent<MeshRenderer>().material;
            if (HasTextureAnimation()) {
                _changeTimer = new CooldownTimer(1.0f / _framerate);
            }
        }
        
        void Update() {
            _material.mainTextureOffset += _offsetRate * Time.deltaTime;
            _material.mainTextureScale += _scaleRate * Time.deltaTime;

            if (_changeTimer != null && _changeTimer.Use()) {
                _curTexture = (_curTexture + 1) % _textures.Length;
                _material.mainTexture = _textures[_curTexture];
                _material.SetTexture("_EmissionMap", _textures[_curTexture]);
            }
        }
    }

}
