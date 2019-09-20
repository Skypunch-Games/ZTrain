using UnityEngine;
using System.Globalization;
using System.Text;

namespace com.ootii.Data.Serializers
{
    public static class SerializationHelper 
    {
        //public static string SerializeValue(string lProperty, object lValue)
        //{
        //    StringBuilder lJSON = new StringBuilder();
        //    lJSON.Append("\"" + lProperty + "\" : ");



        //    return lJSON.ToString();
        //}


        public static string Serialize(this float lValue)
        {
            return lValue.ToString("G8", CultureInfo.InvariantCulture);
        }

        public static string Serialize(this int lValue)
        {
            return lValue.ToString(CultureInfo.InvariantCulture);
        }

        public static string Serialize(this Vector2 lValue)
        {
            return "(" + lValue.x.Serialize() + "," + lValue.y.Serialize() + ")";
        }

        public static string Serialize(this Vector3 lValue)
        {
            return "(" + lValue.x.Serialize() + ", " + lValue.y.Serialize() + ", " + lValue.z.Serialize() + ")";            
        }

        public static string Serialize(this Vector4 lValue)
        {
            return "(" + lValue.x.Serialize() + ", " + lValue.y.Serialize() + ", " + lValue.z.Serialize() + ", " + lValue.w.Serialize() + ")";            
        }

        public static string Serialize(this Quaternion lValue)
        {
            return "(" + lValue.x.Serialize() + ", " + lValue.y.Serialize() + ", " + lValue.z.Serialize() + ", " + lValue.w.Serialize() + ")";            
        }
    }
}
