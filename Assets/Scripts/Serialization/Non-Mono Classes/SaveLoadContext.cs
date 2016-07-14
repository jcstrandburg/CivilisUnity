using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

/// <summary>
/// Helper classes for saving/loading games. Provides a repository for data needed by 
/// data serialization functions, plus some helper functions for determining which fields
/// too save
/// </summary>
public class SaveLoadContext {
    public Dictionary<string, GameObject> prefabDictionary = new Dictionary<string, GameObject>();
    public Dictionary<string, ObjectIdentifier> oidReferences = new Dictionary<string, ObjectIdentifier>();
    public List<IReferenceResolver> refResolvers = new List<IReferenceResolver>();
    public List<string> autoSerializeTypes = new List<string>();


    /// <summary>
    /// Determines if the given field can be serlialized either by standard libraries or 
    /// by our custom serializeation code
    /// </summary>
    /// <param name="field"></param>
    /// <returns></returns>
    public bool FieldSerializeable(FieldInfo field) {
        if (field.GetCustomAttributes(typeof(ForceSerialize), false).Length > 0)
            return true;
        return TypeSerializeable(field.FieldType);
    }

    /// <summary>
    /// Determines if the given type can be serlialized either by standard libraries or 
    /// by our custom serializeation code
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public bool TypeSerializeable(Type t) {
        if (TypeSystem.IsEnumerableType(t) || TypeSystem.IsCollectionType(t)) {
            Type t2 = TypeSystem.GetElementType(t);
            return t2.IsSerializable || autoSerializeTypes.Contains(t2.Name);
            //return TypeSerializeable(TypeSystem.GetElementType(t)); //this causes stack overflow on some collection types (e.g. dictionarys), bears examination if we ever want deeply nested enumerable/collections serialized
        }
        return t.IsSerializable || autoSerializeTypes.Contains(t.Name);
    }
}
