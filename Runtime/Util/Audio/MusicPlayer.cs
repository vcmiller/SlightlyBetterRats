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

using Infohazard.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace SBR {
    /// <summary>
    /// Class for playing music in a variety of different situations.
    /// Can persist through level loads, so music isn't interrupted when loading or reloading a scene.
    /// Can fade to a new track, or switch abruptly.
    /// Can play AudioSources on child GameObjects in order, or shuffled.
    /// </summary>
    public class MusicPlayer : MonoBehaviour {
        public enum ConflictResolution {
            DestroyNewer, DestroyOlder, DestroyNewerIfSameId
        }

        /// <summary>
        /// Whether to fade out AudioSources when stopping.
        /// </summary>
        [FormerlySerializedAs("fadeOut"),Tooltip("Whether to fade out AudioSources when stopping.")]
        public bool _fadeOut = true;

        /// <summary>
        /// Whether to fade in AudioSources when playing.
        /// </summary>
        [FormerlySerializedAs("fadeIn")]
        public bool _fadeIn = false;

        /// <summary>
        /// Used if a newly loaded MusicPlayer has is resolution mode set to DestroyNewerIfSameId.
        /// </summary>
        [FormerlySerializedAs("id"),Tooltip("Used if a newly loaded MusicPlayer has is resolution mode set to DestroyNewerIfSameId.")]
        public int _id;

        /// <summary>
        /// How to handle cases where a MusicPlayer is loaded when one already exists.
        /// This is most likely to occur when a new level is loaded while persistThroughLoad is true.
        /// </summary>
        [FormerlySerializedAs("conflictResolution"),Tooltip("How to handle cases where a MusicPlayer is loaded when one already exists.")]
        public ConflictResolution _conflictResolution;

        /// <summary>
        /// Whether to shuffle order of tracks.
        /// </summary>
        [FormerlySerializedAs("shuffle"),Tooltip("Whether to shuffle order of tracks.")]
        public bool _shuffle = false;

        /// <summary>
        /// Whether the player should persist and keep playing when a new level is loaded.
        /// If the new level also has a MusicPlayer, one of them will be destroyed,
        /// determined by the newly loaded player's conflictResolution.
        /// </summary>
        [FormerlySerializedAs("persistThroughLoad"),Tooltip("Whether the player should persist and keep playing when a new level is loaded.")]
        public bool _persistThroughLoad = true;

        /// <summary>
        /// Whether to automatically start playing the first track on awake.
        /// </summary>
        [FormerlySerializedAs("playOnAwake"),Tooltip("Whether to automatically start playing the first track on awake.")]
        public bool _playOnAwake = true;

        /// <summary>
        /// Currently active MusicPlayer.
        /// </summary>
        public static MusicPlayer Inst { get; private set; }
        
        /// <summary>
        /// Index of currently playing AudioSource.
        /// </summary>
        public int CurrentIndex { get; private set; }

        /// <summary>
        /// Whether is currently playing any audioSource.
        /// </summary>
        public bool IsPlaying { get; private set; }

        private AudioSource _currentSource;
        private AudioSource[] _audioSources;
        private bool _started = false;

        private void OnValidate() {
            if (_fadeOut) {
                foreach (AudioSource source in GetComponentsInChildren<AudioSource>()) {
                    if (!source.GetComponent<AudioFade>()) {
                        source.gameObject.AddComponent<AudioFade>();
                    }
                }
            }
        }

        private void Awake() {
            if (Inst == this) return;

            if (Inst != null) {
                bool keepOther = _conflictResolution == ConflictResolution.DestroyNewer || 
                    (_id == Inst._id && _conflictResolution == ConflictResolution.DestroyNewerIfSameId);

                
                if (keepOther) {
                    gameObject.SetActive(false);
                    return;
                } else {
                    Inst.Stop();
                    Spawnable.Despawn(Inst.gameObject, Inst.GetComponent<AudioFade>()._fadeDuration);
                }
            }

            _audioSources = GetComponentsInChildren<AudioSource>();
            CurrentIndex = _shuffle ? UnityEngine.Random.Range(0, _audioSources.Length) : 0;
            Inst = this;
            if (_persistThroughLoad) DontDestroyOnLoad(gameObject);
        }

        private void Start() {
            if (_playOnAwake) Play();
        }

        private void Update() {
            if (IsPlaying && !_started) {
                if (!_currentSource.isPlaying) {
                    _currentSource = null;
                    IsPlaying = false;
                    Next();
                }
            }

            _started = false;
        }

        /// <summary>
        /// Play current track, or resume if paused. 
        /// Will always be run on active instance, so it is safe to use in UnityEvents even if inactive.
        /// </summary>
        public void Play() {
            if (Inst != this) {
                Inst.Play();
                return;
            }

            if (IsPlaying) return;

            if (_currentSource) {
                _currentSource.UnPause();
                IsPlaying = true;
            } else {
                Play(CurrentIndex);
            }
        }

        /// <summary>
        /// Play next (or random) track. 
        /// Will always be run on active instance, so it is safe to use in UnityEvents even if inactive.
        /// </summary>
        public void Next() {
            if (Inst != this) {
                Inst.Next();
                return;
            }

            if (_shuffle) {
                Play(UnityEngine.Random.Range(0, _audioSources.Length));
            } else {
                Play((CurrentIndex + 1) % _audioSources.Length);
            }
        }

        /// <summary>
        /// Play a given AudioSource. Does not need to be in the audioSources list. 
        /// Will always be run on active instance, so it is safe to use in UnityEvents even if inactive.
        /// </summary>
        public void Play(AudioSource source) {
            if (Inst != this) {
                Inst.Play(source);
                return;
            }

            if (IsPlaying) {
                Stop();
            }

            IsPlaying = true;
            _currentSource = source;

            if (_fadeIn) {
                source.FadeIn();
            } else {
                source.ClearFade();
                source.Play();
                _started = true;
            }
        }

        /// <summary>
        /// Play track at given index. 
        /// Will always be run on active instance, so it is safe to use in UnityEvents even if inactive.
        /// </summary>
        public void Play(int index) {
            if (Inst != this) {
                Inst.Play(index);
                return;
            }

            CurrentIndex = index;
            Play(_audioSources[index]);
        }

        /// <summary>
        /// Pause the current track. 
        /// Will always be run on active instance, so it is safe to use in UnityEvents even if inactive.
        /// </summary>
        public void Pause() {
            if (Inst != this) {
                Inst.Pause();
                return;
            }

            _currentSource.Pause();
            IsPlaying = false;
        }

        /// <summary>
        /// Stop the current track. 
        /// Will always be run on active instance, so it is safe to use in UnityEvents even if inactive.
        /// </summary>
        public void Stop() {
            if (Inst != this) {
                Inst.Stop();
                return;
            }

            if (!_currentSource) return;
            
            if (_fadeOut) {
                _currentSource.FadeOut();
            } else {
                _currentSource.Stop();
            }
            _currentSource = null;
        }

        /// <summary>
        /// Same as stop, but additionally resets to beginning of playlist.
        /// </summary>
        public void StopAndReset() {
            if (Inst != this) {
                Inst.StopAndReset();
                return;
            }

            Stop();
            CurrentIndex = 0;
        }
    }

}
