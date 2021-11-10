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
using UnityEngine.Audio;
using System.Text.RegularExpressions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace SBR {
    /// <summary>
    /// Contains several utility functions.
    /// </summary>
    public static class Util {

        private static Assembly[] _allAssemblies;

        /// <summary>
        /// Returns an array of all loaded assemblies.
        /// </summary>
        public static Assembly[] AllAssemblies {
            get {
                if (_allAssemblies == null) {
                    _allAssemblies = AppDomain.CurrentDomain.GetAssemblies();
                }
                return _allAssemblies;
            }
        }

        public static IEnumerable<Type> AllTypes {
            get {
                foreach (Assembly assembly in AllAssemblies) {
                    Type[] types = null;
                    try {
                        types = assembly.GetTypes();
                    } catch (ReflectionTypeLoadException ex) {
                        types = ex.Types;
                    } catch {
                        continue;
                    }

                    foreach (Type type in types) {
                        if (type != null) {
                            yield return type;
                        }
                    }
                }
            }
        }

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

        /// <summary>
        /// Draw the given Bounds in the scene view for one frame.
        /// </summary>
        /// <param name="bounds">Bounds to draw.</param>
        /// <param name="color">Color to draw.</param>
        public static void DrawDebugBounds(Bounds bounds, Color color, float duration = 0.0f, bool depthTest = true) {
            Debug.DrawLine(new Vector3(bounds.min.x, bounds.min.y, bounds.min.z), new Vector3(bounds.max.x, bounds.min.y, bounds.min.z), color, duration, depthTest);
            Debug.DrawLine(new Vector3(bounds.min.x, bounds.max.y, bounds.min.z), new Vector3(bounds.max.x, bounds.max.y, bounds.min.z), color, duration, depthTest);
            Debug.DrawLine(new Vector3(bounds.min.x, bounds.max.y, bounds.max.z), new Vector3(bounds.max.x, bounds.max.y, bounds.max.z), color, duration, depthTest);
            Debug.DrawLine(new Vector3(bounds.min.x, bounds.min.y, bounds.max.z), new Vector3(bounds.max.x, bounds.min.y, bounds.max.z), color, duration, depthTest);
            Debug.DrawLine(new Vector3(bounds.min.x, bounds.min.y, bounds.min.z), new Vector3(bounds.min.x, bounds.max.y, bounds.min.z), color, duration, depthTest);
            Debug.DrawLine(new Vector3(bounds.max.x, bounds.min.y, bounds.min.z), new Vector3(bounds.max.x, bounds.max.y, bounds.min.z), color, duration, depthTest);
            Debug.DrawLine(new Vector3(bounds.max.x, bounds.min.y, bounds.max.z), new Vector3(bounds.max.x, bounds.max.y, bounds.max.z), color, duration, depthTest);
            Debug.DrawLine(new Vector3(bounds.min.x, bounds.min.y, bounds.max.z), new Vector3(bounds.min.x, bounds.max.y, bounds.max.z), color, duration, depthTest);
            Debug.DrawLine(new Vector3(bounds.min.x, bounds.min.y, bounds.min.z), new Vector3(bounds.min.x, bounds.min.y, bounds.max.z), color, duration, depthTest);
            Debug.DrawLine(new Vector3(bounds.max.x, bounds.min.y, bounds.min.z), new Vector3(bounds.max.x, bounds.min.y, bounds.max.z), color, duration, depthTest);
            Debug.DrawLine(new Vector3(bounds.max.x, bounds.max.y, bounds.min.z), new Vector3(bounds.max.x, bounds.max.y, bounds.max.z), color, duration, depthTest);
            Debug.DrawLine(new Vector3(bounds.min.x, bounds.max.y, bounds.min.z), new Vector3(bounds.min.x, bounds.max.y, bounds.max.z), color, duration, depthTest);
        }

        public static void DebugBreakAfterFrames(this MonoBehaviour cmp, int frames) {
            if (frames == 0) {
                Debug.Break();
                return;
            }

            cmp.StartCoroutine(CRT_DebugBreakAfterFrames(frames));
        }

        private static IEnumerator CRT_DebugBreakAfterFrames(int frames) {
            for (int i = 0; i < frames; i++) {
                yield return null;
            }
            Debug.Break();
        }

        /// <summary>
        /// Splits a camel-case string into words separated by spaces.
        /// Multiple consecutive capitals are considered the same word.
        /// </summary>
        /// <remarks>
        /// From stackoverflow:
        /// https://stackoverflow.com/questions/5796383/insert-spaces-between-words-on-a-camel-cased-token/5796793
        /// </remarks>
        /// <param name="str">The string to split.</param>
        /// <param name="capitalizeFirst">Whether to capitalize the first letter.</param>
        /// <returns>The split string.</returns>
        public static string SplitCamelCase(this string str, bool capitalizeFirst = false) {
            string result = Regex.Replace(
                Regex.Replace(
                    str,
                    @"(\P{Ll})(\P{Ll}\p{Ll})",
                    "$1 $2"
                ),
                @"(\p{Ll})(\P{Ll})",
                "$1 $2"
            );

            if (result.Length > 0 && capitalizeFirst) {
                result = result[0].ToString().ToUpper() + result.Substring(1);
            }
            return result;
        }

        public static void SetLayerRecursively(this GameObject obj, int layer) {
            obj.layer = layer;
            for (int i = 0; i < obj.transform.childCount; i++) {
                SetLayerRecursively(obj.transform.GetChild(i).gameObject, layer);
            }
        }

        public static Type GetType(string fullName) {
            if (string.IsNullOrEmpty(fullName)) return null;

            foreach (var item in AllAssemblies) {
                var type = item.GetType(fullName);
                if (type != null) {
                    return type;
                }
            }

            return null;
        }

        public static T DeepCopy<T>(this T obj) {
            using var ms = new MemoryStream();

            var formatter = new BinaryFormatter();
            formatter.Serialize(ms, obj);
            ms.Position = 0;

            return (T) formatter.Deserialize(ms);
        }

        public static string GetRelativeTransformPath(this Transform from, Transform to) {
            StringBuilder builder = new StringBuilder();

            Transform current = to;
            bool first = true;

            while (current && current != from) {
                if (current) {
                    builder.Insert(0, first ? $"/{current.name}" : current.name);
                }
                current = current.parent;
                first = false;
            }

            if (current != from) {
                Debug.LogError($"GetRelativeTransformPath did not find parent {from.name}.");
                return null;
            }

            return builder.ToString();
        }

        public static Transform GetTransformAtRelativePath(this Transform from, string path) {
            string[] parts = path.Split('/');
            Transform current = from;

            foreach (string part in parts) {
                current = from.Find(part);
                if (current == null) return null;
            }

            return current;
        }

        private static byte[] _randBuf = new byte[8];
        public static long NextLong(this System.Random random, long min, long max) {
            ulong uRange = (ulong)(max - min);
            ulong ulongRand;
            do
            {
                random.NextBytes(_randBuf);
                ulongRand = (ulong)BitConverter.ToInt64(_randBuf, 0);
            } while (ulongRand > ulong.MaxValue - ((ulong.MaxValue % uRange) + 1) % uRange);

            return (long)(ulongRand % uRange) + min;
        }

        public static ulong NextUlong(this System.Random random) {
            random.NextBytes(_randBuf);
            return BitConverter.ToUInt64(_randBuf, 0);
        }

        public delegate bool SelectWhereDelegate<in T1, T2>(T1 input, out T2 output);
        public static IEnumerable<T2> SelectWhere<T1, T2>(this IEnumerable<T1> input,
                                                          SelectWhereDelegate<T1, T2> selectionDelegate) {
            foreach (T1 item in input) {
                if (selectionDelegate(item, out T2 value)) yield return value;
            }
        }

        public static T2 FirstOrDefaultWhere<T1, T2>(this IEnumerable<T1> input,
                                                          SelectWhereDelegate<T1, T2> selectionDelegate) {
            foreach (T1 item in input) {
                if (selectionDelegate(item, out T2 value)) return value;
            }

            return default;
        }
    }
}