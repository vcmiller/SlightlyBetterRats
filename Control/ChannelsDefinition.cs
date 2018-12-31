using System;
using System.Collections.Generic;
using UnityEngine;

namespace SBR {
    /// <summary>
    /// Used to define input channels and then generate the class.
    /// </summary>
    /// <remarks>
    /// Channels classes can also be created manually, but that should be unnecessary in most cases.
    /// </remarks>
    [CreateAssetMenu(menuName = "Channels Definition")]
    public class ChannelsDefinition : ScriptableObject {
        [Serializable]
        public class Channel {
            /// <summary>
            /// Name of the property for accessing the channel.
            /// </summary>
            [Tooltip("Name of the property for accessing the channel.")]
            public string name;

            /// <summary>
            /// Type of data stored in the channel.
            /// </summary>
            [Tooltip("Type of data stored in the channel.")]
            public ChannelType type;

            /// <summary>
            /// Whether the channel's value should reset to the default every frame.
            /// </summary>
            [Tooltip("Whether the channel's value should reset to the default every frame.")]
            public bool clears = false;

            /// <summary>
            /// Whether the float value of this channel has a limited range.
            /// </summary>
            [Tooltip("Whether the float value of this channel has a limited range.")]
            public bool floatHasRange = false;

            /// <summary>
            /// Minimum value of the float range of this channel.
            /// </summary>
            [Tooltip("Minimum value of the float range of this channel.")]
            public float floatMin = float.NegativeInfinity;
            
            /// <summary>
            /// Maximum value of the float range of this channel.
            /// </summary>
            [Tooltip("Maximum value of the float range of this channel.")]
            public float floatMax = float.PositiveInfinity;

            /// <summary>
            /// Default float value of this channel.
            /// </summary>
            [Tooltip("Default float value of this channel.")]
            public float defaultFloat = 0;

            /// <summary>
            /// Whether the int value of this channel has a limited range.
            /// </summary>
            [Tooltip("Whether the int value of this channel has a limited range.")]
            public bool intHasRange = false;

            /// <summary>
            /// Minimum value of the int range of this channel.
            /// </summary>
            [Tooltip("Minimum value of the int range of this channel.")]
            public int intMin = int.MaxValue;

            /// <summary>
            /// Maximum value of the int range of this channel.
            /// </summary>
            [Tooltip("Maximum value of the int range of this channel.")]
            public int intMax = int.MinValue;

            /// <summary>
            /// Default int value of this channel.
            /// </summary>
            [Tooltip("Default int value of this channel.")]
            public int defaultInt = 0;

            /// <summary>
            /// Default bool value of this channel.
            /// </summary>
            [Tooltip("Default bool value of this channel.")]
            public bool defaultBool = false;

            /// <summary>
            /// Whether the int value of this channel has a limited length.
            /// </summary>
            [Tooltip("Whether the Vector value of this channel has a limited range.")]
            public bool vectorHasMax = false;

            /// <summary>
            /// Maximum length of the vector of this channel.
            /// </summary>
            [Tooltip("Maximum length of the vector of this channel.")]
            public float vectorMax;

            /// <summary>
            /// Default Vector value of this channel.
            /// </summary>
            [Tooltip("Default Vector value of this channel.")]
            public Vector3 defaultVector = Vector3.zero;

            /// <summary>
            /// Default rotation value of this channel (euler).
            /// </summary>
            [Tooltip("Default rotation value of this channel (euler).")]
            public Vector3 defaultRotation = Vector3.zero;

            /// <summary>
            /// Custom object type for this channel.
            /// </summary>
            [Tooltip("Custom object type for this channel.")]
            public string objectType;
        }

        public enum ChannelType {
            Float, Int, Bool, Vector, Quaternion, Object
        }

        /// <summary>
        /// Base class to use when generating class.
        /// </summary>
        [TypeSelect(typeof(Channels), true)]
        public string baseClass;

        /// <summary>
        /// All input channels.
        /// </summary>
        public List<Channel> channels;
    }
}