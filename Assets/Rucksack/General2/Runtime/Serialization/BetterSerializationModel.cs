#pragma warning disable 612

using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Devdog.General2
{
    public class BetterSerializationModel
    {
        [NonSerialized]
        private bool _isSerializing = false;

//        public void Save(ref List<UnityEngine.Object> objectReferences, ref string json, UnityEngine.Object obj)
//        {
//            // Ignored; Odin takes over
//        }

        public void Load(ref List<UnityEngine.Object> objectReferences, ref string json, UnityEngine.Object obj)
        {
            if (_isSerializing)
                return;

            _isSerializing = true;
            JsonSerializerObsolete.DeserializeTo(obj, obj.GetType(), json, objectReferences);
            _isSerializing = false;
        }
    }
}

#pragma warning restore 612
