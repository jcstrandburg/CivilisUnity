using System.Runtime.Serialization;
using UnityEngine;

namespace Neolithica.Serialization.Surrogates {
    /// <summary>
    /// Part of the SerializeHelper package by Cherno.
    /// http://forum.unity3d.com/threads/serializehelper-free-save-and-load-utility-de-serialize-all-objects-in-your-scene.338148/
    /// </summary>
    sealed class ColorSurrogate : ISerializationSurrogate {
	
        // Method called to serialize a Vector3 object
        public void GetObjectData(System.Object obj,
            SerializationInfo info, StreamingContext context) {
		
            Color color = (Color) obj;
            info.AddValue("r", color.r);
            info.AddValue("g", color.g);
            info.AddValue("b", color.b);
            info.AddValue("a", color.a);
        }
	
        // Method called to deserialize a Vector3 object
        public System.Object SetObjectData(System.Object obj,
            SerializationInfo info, StreamingContext context,
            ISurrogateSelector selector) {
		
            Color color = (Color) obj;
            color.r = (float)info.GetValue("r", typeof(float));
            color.g = (float)info.GetValue("g", typeof(float));
            color.b = (float)info.GetValue("b", typeof(float));
            color.a = (float)info.GetValue("a", typeof(float));
            obj = color;
            return obj;
        }
    }
}