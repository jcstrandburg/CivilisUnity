using System;

/// <summary>
/// Part of the SerializeHelper package by Cherno.
/// http://forum.unity3d.com/threads/serializehelper-free-save-and-load-utility-de-serialize-all-objects-in-your-scene.338148/
/// A field with this attribute will not get packed into the SaveGame object that gets passed to the serialization code.
/// This attribute will not prevent serialization code if a field of this type is passed to a serializer.
/// As currently implemented (12/13/2016) this attribute is only detected on fields belonging to MonoBehaviour classes.
/// </summary>
public class DontSaveField : Attribute {
	
}
