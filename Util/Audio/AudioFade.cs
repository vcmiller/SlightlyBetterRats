// MIT License
// 
// Copyright (c) 2020 Vincent Miller
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
        [Tooltip("Duration of the fade. Can be overridden by duration parameter to FadeTo.")]
        public float fadeDuration = 1;

        /// <summary>
        /// Current fade level. Controls how loud it starts at.
        /// </summary>
        [Tooltip("Current fade level. Controls how loud it starts at.")]
        [Range(0, 1)]
        public float fadeLevel = 1;

        /// <summary>
        /// Specifies what happens when the script fades to zero. 
        /// </summary>
        [Tooltip("Specifies what happens when the script fades to zero. ")]
        public ZeroBehavior zeroBehavior = ZeroBehavior.Stop;

        private AudioSource audioSource;
        private float currentTarget;
        private float startVolume;
        private float fadeSpeed;
        private bool fading;

        public bool IsFadedIn => currentTarget > 0;

        /// <summary>
        /// Fade the volume to its initial volume, and play if not already playing.
        /// </summary>
        public void FadeIn() {
            fadeLevel = 0;
            FadeTo(1, fadeDuration);
        }

        /// <summary>
        /// Fade the volume to 0, and follow zeroBehavior when it reaches 0.
        /// </summary>
        public void FadeOut() {
            fadeLevel = 1;
            FadeTo(0, fadeDuration);
        }

        /// <summary>
        /// Fade the volume to a given level.
        /// </summary>
        /// <param name="level">The level to fade to.</param>
        public void FadeTo(float level) => FadeTo(level, fadeDuration);

        /// <summary>
        /// Fade the volume to a given level over a given duration.
        /// </summary>
        /// <param name="level">The level to fade to.</param>
        /// <param name="duration">Override for fadeDuration field.</param>
        public void FadeTo(float level, float duration) {
            if (!audioSource.isPlaying) {
                audioSource.volume = fadeLevel * startVolume;
                audioSource.Play();
            }

            currentTarget = Mathf.Clamp01(level);
            fadeSpeed = Mathf.Abs(currentTarget - fadeLevel) / duration;
            fading = true;
        }

        /// <summary>
        /// Clear the fade (reset volume to 1).
        /// </summary>
        public void Clear() {
            fadeLevel = 1;
            audioSource.volume = startVolume;
        }

        private void Awake() {
            audioSource = GetComponent<AudioSource>();
            startVolume = audioSource.volume;
            audioSource.volume = startVolume * fadeLevel;
            if (fadeLevel == 0) {
                ReachedZero();
            }
        }

        private void Update() {
            if (fading) {
                fadeLevel = Mathf.MoveTowards(fadeLevel, currentTarget, fadeSpeed * Time.unscaledDeltaTime);
                audioSource.volume = fadeLevel * startVolume;

                if (fadeLevel == currentTarget) {
                    fading = false;
                    if (fadeLevel == 0) {
                        ReachedZero();
                    }
                }
            }
        }

        private void ReachedZero() {
            if (zeroBehavior == ZeroBehavior.Stop) {
                audioSource.Stop();
            } else if (zeroBehavior == ZeroBehavior.Pause) {
                audioSource.Pause();
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
            if (source.TryGetComponent<AudioFade>(out var fade)) {
                fade.Clear();
            }
        }
    }
}