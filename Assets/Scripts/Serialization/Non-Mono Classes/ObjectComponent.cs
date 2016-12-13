//The ObjectComponent class holds all data of a gameobject's component.
//The Dictionary holds the actual data of a component; A field's name as key and the corresponding value (object) as value. Confusing, right?

using System.Collections.Generic;

/// <summary>
/// Part of the SerializeHelper package by Cherno.
/// http://forum.unity3d.com/threads/serializehelper-free-save-and-load-utility-de-serialize-all-objects-in-your-scene.338148/
/// </summary>
[System.Serializable]
public class ObjectComponent {
	public string componentName;
	public Dictionary<string,object> fields;
    public Dictionary<string, UnityObjectReference> references;
    //public Dictionary<string, CustomSerializedObject> customReferences;
    //Wpublic Dictionary<string, >

    public ObjectComponent() {
        fields = new Dictionary<string, object>();
        references = new Dictionary<string, UnityObjectReference>();
        //customReferences = new Dictionary<string, CustomSerializedObject>();

    }
}