using System.Runtime.Serialization;
using UnityEngine;

/// <summary>
/// Part of the SerializeHelper package by Cherno.
/// http://forum.unity3d.com/threads/serializehelper-free-save-and-load-utility-de-serialize-all-objects-in-your-scene.338148/
/// </summary>
sealed class TransformSurrogate : ISerializationSurrogate {
	
	// Method called to serialize a Vector3 object
	public void GetObjectData(System.Object obj,
	                          SerializationInfo info, StreamingContext context) {
		
		//Transform tr = (Transform) obj;
	}
	
	// Method called to deserialize a Vector3 object
	public System.Object SetObjectData(System.Object obj,
	                                   SerializationInfo info, StreamingContext context,
	                                   ISurrogateSelector selector) {
		
		//Transform tr = (Transform) obj;
		
		return obj;
	}
}