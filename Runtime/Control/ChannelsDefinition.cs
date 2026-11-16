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

using System;
using System.Collections.Generic;
using Infohazard.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace SBR {
    /// <summary>
    /// Used to define input channels and then generate the class.
    /// </summary>
    /// <remarks>
    /// Channels classes can also be created manually, but that should be unnecessary in most cases.
    /// </remarks>
    [CreateAssetMenu(menuName = "Infohazard/Channels Definition")]
    public class ChannelsDefinition : ScriptableObject {
        [Serializable]
        public class Channel {
            /// <summary>
            /// Name of the property for accessing the channel.
            /// </summary>
            [FormerlySerializedAs("name"),Tooltip("Name of the property for accessing the channel.")]
            public string _name;

            /// <summary>
            /// Type of data stored in the channel.
            /// </summary>
            [FormerlySerializedAs("type"),Tooltip("Type of data stored in the channel.")]
            public ChannelType _type;

            /// <summary>
            /// Whether the channel's value should reset to the default every frame.
            /// </summary>
            [FormerlySerializedAs("clears"),Tooltip("Whether the channel's value should reset to the default every frame.")]
            public bool _clears = false;

            /// <summary>
            /// Whether the float value of this channel has a limited range.
            /// </summary>
            [FormerlySerializedAs("floatHasRange"),Tooltip("Whether the float value of this channel has a limited range.")]
            public bool _floatHasRange = false;

            /// <summary>
            /// Minimum value of the float range of this channel.
            /// </summary>
            [FormerlySerializedAs("floatMin"),Tooltip("Minimum value of the float range of this channel.")]
            public float _floatMin = float.NegativeInfinity;
            
            /// <summary>
            /// Maximum value of the float range of this channel.
            /// </summary>
            [FormerlySerializedAs("floatMax"),Tooltip("Maximum value of the float range of this channel.")]
            public float _floatMax = float.PositiveInfinity;

            /// <summary>
            /// Default float value of this channel.
            /// </summary>
            [FormerlySerializedAs("defaultFloat"),Tooltip("Default float value of this channel.")]
            public float _defaultFloat = 0;

            /// <summary>
            /// Whether the int value of this channel has a limited range.
            /// </summary>
            [FormerlySerializedAs("intHasRange"),Tooltip("Whether the int value of this channel has a limited range.")]
            public bool _intHasRange = false;

            /// <summary>
            /// Minimum value of the int range of this channel.
            /// </summary>
            [FormerlySerializedAs("intMin"),Tooltip("Minimum value of the int range of this channel.")]
            public int _intMin = int.MaxValue;

            /// <summary>
            /// Maximum value of the int range of this channel.
            /// </summary>
            [FormerlySerializedAs("intMax"),Tooltip("Maximum value of the int range of this channel.")]
            public int _intMax = int.MinValue;

            /// <summary>
            /// Default int value of this channel.
            /// </summary>
            [FormerlySerializedAs("defaultInt"),Tooltip("Default int value of this channel.")]
            public int _defaultInt = 0;

            /// <summary>
            /// Default bool value of this channel.
            /// </summary>
            [FormerlySerializedAs("defaultBool"),Tooltip("Default bool value of this channel.")]
            public bool _defaultBool = false;

            /// <summary>
            /// Whether the int value of this channel has a limited length.
            /// </summary>
            [FormerlySerializedAs("vectorHasMax"),Tooltip("Whether the Vector value of this channel has a limited range.")]
            public bool _vectorHasMax = false;

            /// <summary>
            /// Maximum length of the vector of this channel.
            /// </summary>
            [FormerlySerializedAs("vectorMax"),Tooltip("Maximum length of the vector of this channel.")]
            public float _vectorMax;

            /// <summary>
            /// Default Vector value of this channel.
            /// </summary>
            [FormerlySerializedAs("defaultVector"),Tooltip("Default Vector value of this channel.")]
            public Vector3 _defaultVector = Vector3.zero;

            /// <summary>
            /// Default rotation value of this channel (euler).
            /// </summary>
            [FormerlySerializedAs("defaultRotation"),Tooltip("Default rotation value of this channel (euler).")]
            public Vector3 _defaultRotation = Vector3.zero;

            /// <summary>
            /// Custom object type for this channel.
            /// </summary>
            [FormerlySerializedAs("objectType"),Tooltip("Custom object type for this channel.")]
            public string _objectType;
        }

        public enum ChannelType {
            Float, Int, Bool, Vector, Quaternion, Object
        }

        /// <summary>
        /// Base class to use when generating class.
        /// </summary>
        [FormerlySerializedAs("baseClass"),TypeSelect(typeof(Channels), true)]
        public string _baseClass;

        /// <summary>
        /// All input channels.
        /// </summary>
        [FormerlySerializedAs("channels")]
        public List<Channel> _channels;
    }
}
