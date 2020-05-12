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

using System;
using System.Collections.Generic;
using UnityEngine;

namespace SBR {
#if !TagsGenerated
    /// <summary>
    /// The default Tag enum. This should be overwritten using Assets > Update Tag Enum.
    /// </summary>
    [Flags]
    public enum Tag {
        Untagged = 1, Respawn = 2, Finish = 4, EditorOnly = 8, MainCamera = 16, Player = 32, GameController = 64
    }
#endif

    /// <summary>
    /// Static operations on Tag enum values.
    /// </summary>
    public static class Tags {
        /// <summary>
        /// Return true if GameObject's tag matches given any tag in given value.
        /// </summary>
        /// <param name="obj">The GameObject to check.</param>
        /// <param name="tag">The tag to compare, which may be multiple tags.</param>
        /// <returns>Whether GameObject matches given tag.</returns>
        public static bool CompareTag(this GameObject obj, Tag tag) {
            Tag objTag = (Tag)Enum.Parse(typeof(Tag), obj.tag);
            return (objTag & tag) != 0;
        }

        /// <summary>
        /// Return true if Component's tag matches given any tag in given value.
        /// </summary>
        /// <param name="cmpnt">The Component to check.</param>
        /// <param name="tag">The tag to compare, which may be multiple tags.</param>
        /// <returns>Whether Component matches given tag.</returns>
        public static bool CompareTag(this Component cmpnt, Tag tag) {
            return cmpnt.gameObject.CompareTag(tag);
        }

        /// <summary>
        /// Set the tag of a GameObject.
        /// </summary>
        /// <param name="obj">Object to modify.</param>
        /// <param name="tag">Tag to set.</param>
        public static void SetTag(this GameObject obj, Tag tag) {
            obj.tag = tag.ToString();
        }
        
        /// <summary>
        /// Get the tag of a GameObject.
        /// </summary>
        /// <param name="obj">Object to read.</param>
        /// <returns>The GameObject's tag.</returns>
        public static Tag GetTag(this GameObject obj) {
            return (Tag)Enum.Parse(typeof(Tag), obj.tag);
        }

        /// <summary>
        /// Set the tag of a Component.
        /// </summary>
        /// <param name="cmpnt">Component to modify.</param>
        /// <param name="tag">Tag to set.</param>
        public static void SetTag(this Component cmpnt, Tag tag) {
            cmpnt.gameObject.SetTag(tag);
        }

        /// <summary>
        /// Get the tag of a Component.
        /// </summary>
        /// <param name="cmpnt">Component to read.</param>
        /// <returns>The Component's tag.</returns>
        public static Tag GetTag(this Component cmpnt) {
            return cmpnt.gameObject.GetTag();
        }

        /// <summary>
        /// Find a single GameObject that matches the given tag.
        /// </summary>
        /// <param name="tag">The tag to match. Can be multiple tags.</param>
        /// <returns>A GameObject matching the given tag, or null.</returns>
        public static GameObject FindGameObjectWithTag(Tag tag) {
            for (int i = 0; i < 32; i++) {
                int index = 1 << i;
                if (!Enum.IsDefined(typeof(Tag), index)) {
                    break;
                }

                Tag t = (Tag)index;
                if ((t & tag) != 0) {
                    var obj = GameObject.FindGameObjectWithTag(t.ToString());
                    if (obj) {
                        return obj;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Find all GameObjects that match the given tag or set of tags.
        /// Returns an IEnumerable to avoid additional allocations,
        /// but Linq can be used to convert it to an array if needed.
        /// </summary>
        /// <param name="tag">The tag to match. Can be multiple tags.</param>
        /// <returns>All of the GameObjects matching that tag.</returns>
        public static IEnumerable<GameObject> FindGameObjectsWithTag(Tag tag) {
            for (int i = 0; i < 32; i++) {
                int index = 1 << i;
                if (!Enum.IsDefined(typeof(Tag), index)) {
                    break;
                }

                Tag t = (Tag)index;
                if ((t & tag) != 0) {
                    foreach (var item in GameObject.FindGameObjectsWithTag(t.ToString())) {
                        yield return item;
                    }
                }
            }
        }
    }
}
