using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

using UnityEngine;

namespace SBR.Persistence {
    public class BinaryFormatterSerializer : Serializer {
        private BinaryFormatter _formatter = new BinaryFormatter();
        
        public override void Write(Stream stream, object data) {
            _formatter.Serialize(stream, data);
        }

        public override bool Read<T>(Stream stream, out T data) {
            object result = _formatter.Deserialize(stream);
            if (result is T resultT) {
                data = resultT;
                return true;
            } else {
                data = default;
                return false;
            }
        }
    }
}