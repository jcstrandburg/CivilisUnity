using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class SaveLoadContext {
    public Dictionary<string, GameObject> prefabDictionary = new Dictionary<string, GameObject>();
    public Dictionary<string, ObjectIdentifier> oidReferences = new Dictionary<string, ObjectIdentifier>();
    public List<IReferenceResolver> refResolvers = new List<IReferenceResolver>();
    public List<string> autoSerilizeTypes = new List<string>();

    public bool FieldSerializeable(FieldInfo field) {
        if (field.GetCustomAttributes(typeof(ForceSerialize), false).Length > 0)
            return true;
        return TypeSerializeable(field.FieldType);
    }

    public bool TypeSerializeable(Type t) {
        if (TypeSystem.IsEnumerableType(t) || TypeSystem.IsCollectionType(t)) {
            Debug.Log(t.Name);
            Type t2 = TypeSystem.GetElementType(t);
            return t.IsSerializable || autoSerilizeTypes.Contains(t.Name);
            //return TypeSerializeable(TypeSystem.GetElementType(t)); //this causes stack overflow, bears examination if we ever want deeply nested enumerable/collections serialized
        }
        return t.IsSerializable || autoSerilizeTypes.Contains(t.Name);
    }
}
