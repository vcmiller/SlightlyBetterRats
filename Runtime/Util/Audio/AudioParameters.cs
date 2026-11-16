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

using System.Collections.Generic;
using Infohazard.Core;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;

namespace SBR {
    /// <summary>
    /// Encapsulates many common parameters used when playing audio clips into a single field.
    /// </summary>
    [System.Serializable]
    public class AudioParameters {
        /// <summary>
        /// Clips to randomly select from.
        /// </summary>
        [FormerlySerializedAs("clips"),Tooltip("Clips to randomly select from.")]
        public AudioClip[] _clips;

        /// <summary>
        /// Volume to play at.
        /// </summary>
        [FormerlySerializedAs("volume"),Range(0.0f, 1.0f)]
        [Tooltip("Volume to play at.")]
        public float _volume = 1;

        /// <summary>
        /// Pitch to play at.
        /// </summary>
        [FormerlySerializedAs("pitch"),Tooltip("Pitch to play at.")]
        public float _pitch = 1;

        /// <summary>
        /// Spatial blend as set on AudioSource. A value of 1 means fully spatialized, 0 means 2D.
        /// </summary>
        [FormerlySerializedAs("spaital"),Tooltip("Spatial blend as set on AudioSource. A value of 1 means fully spatialized, 0 means 2D.")]
        [Range(0.0f, 1.0f)]
        public float _spaital = 0;

        /// <summary>
        /// Whether to loop the sound.
        /// </summary>
        [FormerlySerializedAs("loop"),Tooltip("Whether to loop the sound.")]
        public bool _loop = false;

        /// <summary>
        /// Output AudioMixerGroup.
        /// </summary>
        [FormerlySerializedAs("outputGroup"),Tooltip("Output AudioMixerGroup.")]
        public AudioMixerGroup _outputGroup;

        /// <summary>
        /// Cooldown between repeating the sound clip. If cooldownId is set, this is a global cooldown for all instances with the same ID.
        /// </summary>
        [FormerlySerializedAs("playCooldown"),Tooltip("Cooldown between repeating the sound clip. If cooldownId is set, this is a global cooldown for all instances with the same ID.")]
        public float _playCooldown = 0;

        /// <summary>
        /// ID to use for cooldowns. Instances with the same ID share the same cooldown. If empty, cooldown is specific to this instance.
        /// </summary>
        [FormerlySerializedAs("cooldownId"),Tooltip("ID to use for cooldowns. Instances with the same ID share the same cooldown. If empty, cooldown is specific to this instance.")]
        [ConditionalDraw(nameof(_playCooldown), 0.0f, false)]
        public string _cooldownId;

        private CooldownTimer _playTimer;
        private static Dictionary<string, float> _lastPlayTimes = new Dictionary<string, float>();

        private bool CanPlay() {
            if (_playCooldown == 0) {
                return true;
            } else if (string.IsNullOrEmpty(_cooldownId)) {
                if (_playTimer == null) {
                    _playTimer = new CooldownTimer(_playCooldown, 0);
                }
                return _playTimer.Use();
            } else {
                if (!_lastPlayTimes.ContainsKey(_cooldownId) || Time.time - _lastPlayTimes[_cooldownId] > _playCooldown) {
                    _lastPlayTimes[_cooldownId] = Time.time;
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
            if (_clips == null || _clips.Length == 0) {
                Debug.LogError("Trying to play AudioInfo with no clips!");
                return null;
            }

            if (CanPlay()) {
                AudioClip clip = _clips[Random.Range(0, _clips.Length)];
                return Util.PlayClipAtPoint(clip, point, _volume, _spaital, _pitch, _loop, _outputGroup, attach);
            } else {
                return null;
            }
        }

        /// <summary>
        /// Play at the origin (0, 0, 0).
        /// </summary>
        /// <returns>The spawned AudioSource.</returns>
        public AudioSource Play() {
            AudioSource src = PlayAtPoint(Vector3.zero);
            if (src) src.spatialBlend = 0.0f;
            return src;
        }

        /// <summary>
        /// Play the sound with an existing AudioSource.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="oneShot"></param>
        public void PlayWithSource(AudioSource source, bool oneShot) {
            if (_clips == null || _clips.Length == 0) {
                Debug.LogError("Trying to play AudioInfo with no clips!");
                return;
            }

            if (CanPlay()) {
                AudioClip clip = _clips[Random.Range(0, _clips.Length)];
                source.spatialBlend = _spaital;
                source.pitch = _pitch;
                source.loop = _loop;
                source.outputAudioMixerGroup = _outputGroup;

                if (oneShot) {
                    source.PlayOneShot(clip, _volume);
                } else {
                    source.volume = _volume;
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
            return audio._clips != null && audio._clips.Length > 0;
        }
    }
}
