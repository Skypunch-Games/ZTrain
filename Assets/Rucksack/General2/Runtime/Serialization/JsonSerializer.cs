using System;
using System.Collections.Generic;
using Devdog.General2.Serialization;

namespace Devdog.General2
{
    public static class JsonSerializer
    {
        static JsonSerializer()
        {

        }

        public static byte[] SerializeObject<T>(T value, List<UnityEngine.Object> objectReferences)
            where T: UnityEngine.Object
        {
            var bytes = new byte[0];
            UnitySerializationUtility.SerializeUnityObject(value, ref bytes, ref objectReferences, DataFormat.JSON);
            return bytes;
        }
        
        public static void DeserializeObject<T>(T value, byte[] json, List<UnityEngine.Object> objectReferences)
            where T: UnityEngine.Object
        {
            if (json == null || json.Length == 0)
            {
                return;
            }
            
            UnitySerializationUtility.DeserializeUnityObject(value, ref json, ref objectReferences, DataFormat.JSON);
        }
        
        public static byte[] Serialize<T>(T value, List<UnityEngine.Object> objectReferences)
        {
            return SerializationUtility.SerializeValue(value, DataFormat.JSON, out objectReferences);
        }
        
        public static byte[] Serialize(object value, List<UnityEngine.Object> objectReferences)
        {
            return SerializationUtility.SerializeValue(value, DataFormat.JSON, out objectReferences);
        }

        public static T Deserialize<T>(byte[] json, List<UnityEngine.Object> objectReferences)
        {
            if (json == null || json.Length == 0)
            {
                return default(T);
            }
            
            return SerializationUtility.DeserializeValue<T>(json, DataFormat.JSON, objectReferences);
        }
        
        public static object Deserialize(byte[] json, List<UnityEngine.Object> objectReferences)
        {
            if (json == null || json.Length == 0)
            {
                return null;
            }
            
            return SerializationUtility.DeserializeValueWeak(json, DataFormat.JSON, objectReferences);
        }
    }
}