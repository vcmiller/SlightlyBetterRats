using System;
using System.Collections.Generic;
using UnityEngine;

namespace SBR {
#if !TagsGenerated
    [Flags]
    public enum Tag {
        Untagged = 1, Respawn = 2, Finish = 4, EditorOnly = 8, MainCamera = 16, Player = 32, GameController = 64
    }
#endif

    public static class Tags {
        public static bool CompareTag(this GameObject obj, Tag tag) {
            Tag objTag = (Tag)Enum.Parse(typeof(Tag), obj.tag);
            return (objTag & tag) != 0;
        }

        public static bool CompareTag(this Component cmpnt, Tag tag) {
            return cmpnt.gameObject.CompareTag(tag);
        }

        public static void SetTag(this GameObject obj, Tag tag) {
            obj.tag = tag.ToString();
        }
        
        public static Tag GetTag(this GameObject obj) {
            return (Tag)Enum.Parse(typeof(Tag), obj.tag);
        }

        public static void SetTag(this Component cmpnt, Tag tag) {
            cmpnt.gameObject.SetTag(tag);
        }

        public static Tag GetTag(this Component cmpnt) {
            return cmpnt.gameObject.GetTag();
        }

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
