using System.Runtime.Serialization;
using UnityEngine;
using System.Reflection;
using System;

[System.Serializable]
public class CustomSerializedObjectSurrogate : ISerializationSurrogate {

    public void GetObjectData(System.Object obj, SerializationInfo info, StreamingContext context) {
        Type t = obj.GetType();
        SaveLoadContext sContext = context.Context as SaveLoadContext;
        foreach (FieldInfo f in t.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)) {
            if (f.GetCustomAttributes(typeof(DontSaveField), true).Length > 0) {
                continue;
            }

            bool customSerialize = f.FieldType.GetCustomAttributes(
                typeof(CustomSerialize), true).Length > 0;
            bool forceSerialize = f.GetCustomAttributes(
                typeof(ForceSerialize), true).Length > 0;
            bool autoSerialize = sContext.autoSerilizeTypes.Contains(f.FieldType.Name);

            if (f.FieldType == typeof(GameObject)) {
                GameObject g;
                ObjectIdentifier oid;
                UnityObjectReference r;
                if ((g = f.GetValue(obj) as GameObject) != null
                    && (oid = g.GetComponent<ObjectIdentifier>()) != null
                    && oid.HasID) {
                    r = new UnityObjectReference(oid.id, f.FieldType.Name);
                    info.AddValue(f.Name, r);
                }
            } else if (f.FieldType.IsSubclassOf(typeof(MonoBehaviour))) {
                MonoBehaviour mb;
                ObjectIdentifier oid;
                UnityObjectReference r;
                if ((mb = f.GetValue(obj) as MonoBehaviour) != null
                   && (oid = mb.GetComponent<ObjectIdentifier>()) != null
                   && oid.HasID) {
                    r = new UnityObjectReference(oid.id, f.FieldType.Name);
                    info.AddValue(f.Name, r);
                }
            } else if (sContext.FieldSerializeable(f)) {
                info.AddValue(f.Name, f.GetValue(obj));
            } else {
                Debug.Log("Cannot serilize field " + f.Name);
            }
        }
    }

    public System.Object SetObjectData(System.Object obj,
                                       SerializationInfo info, StreamingContext context,
                                       ISurrogateSelector selector) {

        SaveLoadContext sContext = context.Context as SaveLoadContext;
        Type t = obj.GetType();
        BindingFlags flags = 
              BindingFlags.Public 
            | BindingFlags.NonPublic 
            | BindingFlags.Instance 
            | BindingFlags.SetField;
        foreach (SerializationEntry entry in info) {
            FieldInfo f = t.GetField(entry.Name, flags);
            if (f == null) {
                Debug.Log("Unable to find field " + f.Name);
                continue;
            }
            if (f.GetCustomAttributes(typeof(DontSaveField), true).Length > 0) {
                continue;
            }

            bool customSerialize = (f.FieldType.GetCustomAttributes(
                typeof(CustomSerialize), true).Length > 0);
            bool forceSerialize = f.GetCustomAttributes(
                typeof(ForceSerialize), true).Length > 0;
            bool autoSerialize = sContext.autoSerilizeTypes.Contains(f.FieldType.Name);

            if (f.FieldType == typeof(GameObject) ||
                f.FieldType.IsSubclassOf(typeof(MonoBehaviour))) {
                UnityObjectReference r = info.GetValue(f.Name, typeof(UnityObjectReference)) as UnityObjectReference;
                UnityObjectReferenceResolver resolver = new UnityObjectReferenceResolver(
                    f, obj, r, sContext);
                sContext.refResolvers.Add(resolver);
            } else if (sContext.FieldSerializeable(f)) {
                f.SetValue(obj, info.GetValue(f.Name, f.FieldType));
            } else {
                Debug.LogFormat("Found serialized yet unserializable field {0}", f.Name);
            }
        }
        return obj;
    }
}