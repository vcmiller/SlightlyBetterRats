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

using Infohazard.Core;
using UnityEngine;

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
        [Tooltip("Whether to fade out AudioSources when stopping.")]
        public bool fadeOut = true;

        /// <summary>
        /// Whether to fade in AudioSources when playing.
        /// </summary>
        public bool fadeIn = false;

        /// <summary>
        /// Used if a newly loaded MusicPlayer has is resolution mode set to DestroyNewerIfSameId.
        /// </summary>
        [Tooltip("Used if a newly loaded MusicPlayer has is resolution mode set to DestroyNewerIfSameId.")]
        public int id;

        /// <summary>
        /// How to handle cases where a MusicPlayer is loaded when one already exists.
        /// This is most likely to occur when a new level is loaded while persistThroughLoad is true.
        /// </summary>
        [Tooltip("How to handle cases where a MusicPlayer is loaded when one already exists.")]
        public ConflictResolution conflictResolution;

        /// <summary>
        /// Whether to shuffle order of tracks.
        /// </summary>
        [Tooltip("Whether to shuffle order of tracks.")]
        public bool shuffle = false;

        /// <summary>
        /// Whether the player should persist and keep playing when a new level is loaded.
        /// If the new level also has a MusicPlayer, one of them will be destroyed,
        /// determined by the newly loaded player's conflictResolution.
        /// </summary>
        [Tooltip("Whether the player should persist and keep playing when a new level is loaded.")]
        public bool persistThroughLoad = true;

        /// <summary>
        /// Whether to automatically start playing the first track on awake.
        /// </summary>
        [Tooltip("Whether to automatically start playing the first track on awake.")]
        public bool playOnAwake = true;

        /// <summary>
        /// Currently active MusicPlayer.
        /// </summary>
        public static MusicPlayer inst { get; private set; }
        
        /// <summary>
        /// Index of currently playing AudioSource.
        /// </summary>
        public int currentIndex { get; private set; }

        /// <summary>
        /// Whether is currently playing any audioSource.
        /// </summary>
        public bool isPlaying { get; private set; }

        private AudioSource currentSource;
        private AudioSource[] audioSources;
        private bool started = false;

        private void OnValidate() {
            if (fadeOut) {
                foreach (var source in GetComponentsInChildren<AudioSource>()) {
                    if (!source.GetComponent<AudioFade>()) {
                        source.gameObject.AddComponent<AudioFade>();
                    }
                }
            }
        }

        private void Awake() {
            if (inst == this) return;

            if (inst != null) {
                bool keepOther = conflictResolution == ConflictResolution.DestroyNewer || 
                    (id == inst.id && conflictResolution == ConflictResolution.DestroyNewerIfSameId);

                
                if (keepOther) {
                    gameObject.SetActive(false);
                    return;
                } else {
                    inst.Stop();
                    Spawnable.Despawn(inst.gameObject, inst.GetComponent<AudioFade>().fadeDuration);
                }
            }

            audioSources = GetComponentsInChildren<AudioSource>();
            currentIndex = shuffle ? UnityEngine.Random.Range(0, audioSources.Length) : 0;
            inst = this;
            if (persistThroughLoad) DontDestroyOnLoad(gameObject);
        }

        private void Start() {
            if (playOnAwake) Play();
        }

        private void Update() {
            if (isPlaying && !started) {
                if (!currentSource.isPlaying) {
                    currentSource = null;
                    isPlaying = false;
                    Next();
                }
            }

            started = false;
        }

        /// <summary>
        /// Play current track, or resume if paused. 
        /// Will always be run on active instance, so it is safe to use in UnityEvents even if inactive.
        /// </summary>
        public void Play() {
            if (inst != this) {
                inst.Play();
                return;
            }

            if (isPlaying) return;

            if (currentSource) {
                currentSource.UnPause();
                isPlaying = true;
            } else {
                Play(currentIndex);
            }
        }

        /// <summary>
        /// Play next (or random) track. 
        /// Will always be run on active instance, so it is safe to use in UnityEvents even if inactive.
        /// </summary>
        public void Next() {
            if (inst != this) {
                inst.Next();
                return;
            }

            if (shuffle) {
                Play(UnityEngine.Random.Range(0, audioSources.Length));
            } else {
                Play((currentIndex + 1) % audioSources.Length);
            }
        }

        /// <summary>
        /// Play a given AudioSource. Does not need to be in the audioSources list. 
        /// Will always be run on active instance, so it is safe to use in UnityEvents even if inactive.
        /// </summary>
        public void Play(AudioSource source) {
            if (inst != this) {
                inst.Play(source);
                return;
            }

            if (isPlaying) {
                Stop();
            }

            isPlaying = true;
            currentSource = source;

            if (fadeIn) {
                source.FadeIn();
            } else {
                source.ClearFade();
                source.Play();
                started = true;
            }
        }

        /// <summary>
        /// Play track at given index. 
        /// Will always be run on active instance, so it is safe to use in UnityEvents even if inactive.
        /// </summary>
        public void Play(int index) {
            if (inst != this) {
                inst.Play(index);
                return;
            }

            currentIndex = index;
            Play(audioSources[index]);
        }

        /// <summary>
        /// Pause the current track. 
        /// Will always be run on active instance, so it is safe to use in UnityEvents even if inactive.
        /// </summary>
        public void Pause() {
            if (inst != this) {
                inst.Pause();
                return;
            }

            currentSource.Pause();
            isPlaying = false;
        }

        /// <summary>
        /// Stop the current track. 
        /// Will always be run on active instance, so it is safe to use in UnityEvents even if inactive.
        /// </summary>
        public void Stop() {
            if (inst != this) {
                inst.Stop();
                return;
            }

            if (!currentSource) return;
            
            if (fadeOut) {
                currentSource.FadeOut();
            } else {
                currentSource.Stop();
            }
            currentSource = null;
        }

        /// <summary>
        /// Same as stop, but additionally resets to beginning of playlist.
        /// </summary>
        public void StopAndReset() {
            if (inst != this) {
                inst.StopAndReset();
                return;
            }

            Stop();
            currentIndex = 0;
        }
    }

}