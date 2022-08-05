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
using UnityEngine.Audio;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Infohazard.Core;

namespace SBR {
    /// <summary>
    /// Contains several utility functions.
    /// </summary>
    public static class Util {
        /// <summary>
        /// A more versatile version of AudioSource.PlayClipAtPoint.
        /// </summary>
        /// <param name="clip">The clip to play.</param>
        /// <param name="point">The point at which to play.</param>
        /// <param name="volume">The volume to play at.</param>
        /// <param name="spatial">Spatial blend (0 = 2D, 1 = 3D).</param>
        /// <param name="pitch">The pitch to play at.</param>
        /// <param name="loop">Whether to loop the sound.</param>
        /// <param name="output">Output mixer group.</param>
        /// <param name="attach">Transform to attach the spawned AudioSource to.</param>
        /// <returns>The spawned AudioSource.</returns>
        public static AudioSource PlayClipAtPoint(AudioClip clip, Vector3 point, float volume = 1, float spatial = 1, float pitch = 1, bool loop = false, AudioMixerGroup output = null, Transform attach = null) {
            if (clip == null) {
                return null;
            }

            GameObject obj = new GameObject();
            obj.name = "One shot audio (SBR)";
            obj.transform.parent = attach;
            obj.transform.position = point;

            var src = obj.AddComponent<AudioSource>();
            src.clip = clip;
            src.loop = loop;
            src.spatialBlend = spatial;
            src.volume = volume;
            src.pitch = pitch;
            src.outputAudioMixerGroup = output;
            src.Play();

            if (!loop) {
                Spawnable.Despawn(obj, clip.length);
            }

            return src;
        }

        public static T DeepCopy<T>(this T obj) {
            using var ms = new MemoryStream();

            var formatter = new BinaryFormatter();
            formatter.Serialize(ms, obj);
            ms.Position = 0;

            return (T) formatter.Deserialize(ms);
        }
    }
}