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

using System.Collections.Generic;
using Infohazard.Core;
using UnityEngine;
using UnityEngine.Audio;

namespace SBR {
    /// <summary>
    /// Encapsulates many common parameters used when playing audio clips into a single field.
    /// </summary>
    [System.Serializable]
    public class AudioParameters {
        /// <summary>
        /// Clips to randomly select from.
        /// </summary>
        [Tooltip("Clips to randomly select from.")]
        public AudioClip[] clips;

        /// <summary>
        /// Volume to play at.
        /// </summary>
        [Range(0.0f, 1.0f)]
        [Tooltip("Volume to play at.")]
        public float volume = 1;

        /// <summary>
        /// Pitch to play at.
        /// </summary>
        [Tooltip("Pitch to play at.")]
        public float pitch = 1;

        /// <summary>
        /// Spatial blend as set on AudioSource. A value of 1 means fully spatialized, 0 means 2D.
        /// </summary>
        [Tooltip("Spatial blend as set on AudioSource. A value of 1 means fully spatialized, 0 means 2D.")]
        [Range(0.0f, 1.0f)]
        public float spaital = 0;

        /// <summary>
        /// Whether to loop the sound.
        /// </summary>
        [Tooltip("Whether to loop the sound.")]
        public bool loop = false;

        /// <summary>
        /// Output AudioMixerGroup.
        /// </summary>
        [Tooltip("Output AudioMixerGroup.")]
        public AudioMixerGroup outputGroup;

        /// <summary>
        /// Cooldown between repeating the sound clip. If cooldownId is set, this is a global cooldown for all instances with the same ID.
        /// </summary>
        [Tooltip("Cooldown between repeating the sound clip. If cooldownId is set, this is a global cooldown for all instances with the same ID.")]
        public float playCooldown = 0;

        /// <summary>
        /// ID to use for cooldowns. Instances with the same ID share the same cooldown. If empty, cooldown is specific to this instance.
        /// </summary>
        [Tooltip("ID to use for cooldowns. Instances with the same ID share the same cooldown. If empty, cooldown is specific to this instance.")]
        [ConditionalDraw("playCooldown", 0.0f, false)]
        public string cooldownId;

        private CooldownTimer playTimer;
        private static Dictionary<string, float> lastPlayTimes = new Dictionary<string, float>();

        private bool CanPlay() {
            if (playCooldown == 0) {
                return true;
            } else if (string.IsNullOrEmpty(cooldownId)) {
                if (playTimer == null) {
                    playTimer = new CooldownTimer(playCooldown, 0);
                }
                return playTimer.Use();
            } else {
                if (!lastPlayTimes.ContainsKey(cooldownId) || Time.time - lastPlayTimes[cooldownId] > playCooldown) {
                    lastPlayTimes[cooldownId] = Time.time;
                    return true;
                } else {
                    return false;
                }
            }
        }

        /// <summary>
        /// Play the sound at the given point, parenting it to a given transform if not null.
        /// </summary>
        /// <param name="point">The point at which to play the sound.</param>
        /// <param name="attach">An optional transform to attach to.</param>
        /// <returns>The spawned AudioSource.</returns>
        public AudioSource PlayAtPoint(Vector3 point, Transform attach = null) {
            if (clips == null || clips.Length == 0) {
                Debug.LogError("Trying to play AudioInfo with no clips!");
                return null;
            }

            if (CanPlay()) {
                var clip = clips[Random.Range(0, clips.Length)];
                return Util.PlayClipAtPoint(clip, point, volume, spaital, pitch, loop, outputGroup, attach);
            } else {
                return null;
            }
        }

        /// <summary>
        /// Play at the origin (0, 0, 0).
        /// </summary>
        /// <returns>The spawned AudioSource.</returns>
        public AudioSource Play() {
            var src = PlayAtPoint(Vector3.zero);
            if (src) src.spatialBlend = 0.0f;
            return src;
        }

        /// <summary>
        /// Play the sound with an existing AudioSource.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="oneShot"></param>
        public void PlayWithSource(AudioSource source, bool oneShot) {
            if (clips == null || clips.Length == 0) {
                Debug.LogError("Trying to play AudioInfo with no clips!");
                return;
            }

            if (CanPlay()) {
                var clip = clips[Random.Range(0, clips.Length)];
                source.spatialBlend = spaital;
                source.pitch = pitch;
                source.loop = loop;
                source.outputAudioMixerGroup = outputGroup;

                if (oneShot) {
                    source.PlayOneShot(clip, volume);
                } else {
                    source.volume = volume;
                    source.clip = clip;
                    source.Play();
                }
            }
        }

        /// <summary>
        /// Equal to true if there is at least one clip.
        /// </summary>
        /// <param name="audio"></param>
        public static implicit operator bool(AudioParameters audio) {
            return audio.clips != null && audio.clips.Length > 0;
        }
    }
}