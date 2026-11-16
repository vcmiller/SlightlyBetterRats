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
    /// Used to fade in/out an AudioSource.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class AudioFade : MonoBehaviour {
        /// <summary>
        /// Specifies what happens when the script fades to zero.
        /// </summary>
        public enum ZeroBehavior {
            KeepPlaying, Stop, Pause
        }

        /// <summary>
        /// Duration of the fade. Can be overridden by duration parameter to FadeTo.
        /// </summary>
        [FormerlySerializedAs("fadeDuration"),Tooltip("Duration of the fade. Can be overridden by duration parameter to FadeTo.")]
        public float _fadeDuration = 1;

        /// <summary>
        /// Current fade level. Controls how loud it starts at.
        /// </summary>
        [FormerlySerializedAs("fadeLevel"),Tooltip("Current fade level. Controls how loud it starts at.")]
        [Range(0, 1)]
        public float _fadeLevel = 1;

        /// <summary>
        /// Specifies what happens when the script fades to zero. 
        /// </summary>
        [FormerlySerializedAs("zeroBehavior"),Tooltip("Specifies what happens when the script fades to zero. ")]
        public ZeroBehavior _zeroBehavior = ZeroBehavior.Stop;

        private AudioSource _audioSource;
        private float _currentTarget;
        private float _startVolume;
        private float _fadeSpeed;
        private bool _fading;

        public bool IsFadedIn => _currentTarget > 0;

        /// <summary>
        /// Fade the volume to its initial volume, and play if not already playing.
        /// </summary>
        public void FadeIn() {
            _fadeLevel = 0;
            FadeTo(1, _fadeDuration);
        }

        /// <summary>
        /// Fade the volume to 0, and follow zeroBehavior when it reaches 0.
        /// </summary>
        public void FadeOut() {
            _fadeLevel = 1;
            FadeTo(0, _fadeDuration);
        }

        /// <summary>
        /// Fade the volume to a given level.
        /// </summary>
        /// <param name="level">The level to fade to.</param>
        public void FadeTo(float level) => FadeTo(level, _fadeDuration);

        /// <summary>
        /// Fade the volume to a given level over a given duration.
        /// </summary>
        /// <param name="level">The level to fade to.</param>
        /// <param name="duration">Override for fadeDuration field.</param>
        public void FadeTo(float level, float duration) {
            if (!_audioSource.isPlaying) {
                _audioSource.volume = _fadeLevel * _startVolume;
                _audioSource.Play();
            }

            _currentTarget = Mathf.Clamp01(level);
            _fadeSpeed = Mathf.Abs(_currentTarget - _fadeLevel) / duration;
            _fading = true;
        }

        /// <summary>
        /// Clear the fade (reset volume to 1).
        /// </summary>
        public void Clear() {
            _fadeLevel = 1;
            _audioSource.volume = _startVolume;
        }

        private void Awake() {
            _audioSource = GetComponent<AudioSource>();
            _startVolume = _audioSource.volume;
            _audioSource.volume = _startVolume * _fadeLevel;
            if (_fadeLevel == 0) {
                ReachedZero();
            }
        }

        private void Update() {
            if (_fading) {
                _fadeLevel = Mathf.MoveTowards(_fadeLevel, _currentTarget, _fadeSpeed * Time.unscaledDeltaTime);
                _audioSource.volume = _fadeLevel * _startVolume;

                if (_fadeLevel == _currentTarget) {
                    _fading = false;
                    if (_fadeLevel == 0) {
                        ReachedZero();
                    }
                }
            }
        }

        private void ReachedZero() {
            if (_zeroBehavior == ZeroBehavior.Stop) {
                _audioSource.Stop();
            } else if (_zeroBehavior == ZeroBehavior.Pause) {
                _audioSource.Pause();
            }
        }
    }
    
    public static class AudioFadeExtensions {
        /// <summary>
        /// Fade the AudioSource to its initial volume. Requires it to have an AudioFade component.
        /// </summary>
        /// <param name="source"></param>
        public static void FadeIn(this AudioSource source) =>  FadeTo(source, 1);

        /// <summary>
        /// Fade the AudioSource to zero. Requires it to have an AudioFade component.
        /// </summary>
        /// <param name="source"></param>
        public static void FadeOut(this AudioSource source) => FadeTo(source, 0);

        /// <summary>
        /// Fade the AudioSource to given value. Requires it to have an AudioFade component.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="level">The level to fade to.</param>
        public static void FadeTo(this AudioSource source, float level) {
            source.GetComponent<AudioFade>().FadeTo(level);
        }

        /// <summary>
        /// Fade the AudioSource to given value over given duration. 
        /// Requires it to have an AudioFade component.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="level">The level to fade to.</param>
        /// <param name="duration">Duration over which to fade.</param>
        public static void FadeTo(this AudioSource source, float level, float duration) {
            source.GetComponent<AudioFade>().FadeTo(level, duration);
        }

        /// <summary>
        /// Reset the AudioFade on the given AudioSource if there is one.
        /// This should be used if the source was faded out, but now needs to be played without fade.
        /// </summary>
        /// <param name="source"></param>
        public static void ClearFade(this AudioSource source) {
            if (source.TryGetComponent<AudioFade>(out AudioFade fade)) {
                fade.Clear();
            }
        }
    }
}
