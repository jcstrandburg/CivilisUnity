using System.Runtime.Serialization;
using UnityEngine;
using System.Reflection;
using System;

/// <summary>
/// Surrogate for all classes with the CustomSerialize attributeor fields with the ForceSerialize attribute
/// </summary>
[System.Serializable]
public class CustomSerializedObjectSurrogate : ISerializationSurrogate {

    public void GetObjectData(System.Object obj, SerializationInfo info, StreamingContext context) {
        Type t = obj.GetType();
        SaveLoadContext sContext = context.Context as SaveLoadContext;

        //try to serialize each field
        foreach (FieldInfo f in t.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)) {
            if (f.GetCustomAttributes(typeof(DontSaveField), true).Length > 0) {
                continue;
            }

            if (f.FieldType == typeof(GameObject)) {
                //create a resolver for GameObjects since they are not serialized normally
                GameObject g;
                ObjectIdentifier oid;
                UnityObjectReference r;

                //only store the reference if we can get a unique id (only for objects being saved)
                if ((g = f.GetValue(obj) as GameObject) != null
                    && (oid = g.GetComponent<ObjectIdentifier>()) != null
                    && oid.HasID) {
                    r = new UnityObjectReference(oid.id, f.FieldType.Name, g.name);
                    info.AddValue(f.Name, r);
                }
            } else if (f.FieldType.IsSubclassOf(typeof(MonoBehaviour))) {
                //create a resolver for MonoBehaviours since they are not serialized normally
                MonoBehaviour mb;
                ObjectIdentifier oid;
                UnityObjectReference r;

                //only store the reference if we can get a unique id (only for objects being saved)
                if (  (mb = f.GetValue(obj) as MonoBehaviour) != null
                   && (oid = mb.GetComponent<ObjectIdentifier>()) != null
                   && oid.HasID) {
                    r = new UnityObjectReference(oid.id, f.FieldType.Name, mb.name);
                    info.AddValue(f.Name, r);
                }
            } else if (sContext.FieldSerializeable(f)) {
                object value = f.GetValue(obj);
                info.AddValue(f.Name, value);
                // This may not work as intended, for example if this object is a collection or enumeration of serialiazables
                //MethodInfo method = f.FieldType.GetMethod("OnSerialize", new Type[] { });
                //if (method != null) {
                //    method.Invoke(value, new object[]{});
                //}
            } else {
                Debug.Log("Cannot serilize field " + f.Name);
            }
        }
    }

    public System.Object SetObjectData(System.Object obj,
                                       SerializationInfo info, 
                                       StreamingContext context,
                                       ISurrogateSelector selector) {

        SaveLoadContext sContext = context.Context as SaveLoadContext;
        Type t = obj.GetType();
        BindingFlags flags = 
              BindingFlags.Public 
            | BindingFlags.NonPublic 
            | BindingFlags.Instance 
            | BindingFlags.SetField;

        //attempt to extract each serialized field
        foreach (SerializationEntry entry in info) {
            FieldInfo f = t.GetField(entry.Name, flags);
            if (f == null) {
                Debug.Log("Unable to find field " + f.Name);
                continue;
            }
            //skip DontSaveField fields
            if (f.GetCustomAttributes(typeof(DontSaveField), true).Length > 0) {
                continue;
            }

            if (f.FieldType == typeof(GameObject) ||
                f.FieldType.IsSubclassOf(typeof(MonoBehaviour))) 
            {
                //create a resolver for UnityObjects, since they are not serialized like everything else
                UnityObjectReference r = info.GetValue(f.Name, typeof(UnityObjectReference)) as UnityObjectReference;
                UnityObjectReferenceResolver resolver = new UnityObjectReferenceResolver(f, obj, r, sContext);
                sContext.refResolvers.Add(resolver);
            } else if (sContext.FieldSerializeable(f)) {
                object value = info.GetValue(f.Name, f.FieldType);
                f.SetValue(obj, value);
                // This may not work as intended, for example if this object is a collection or enumeration of serialiazables
                //MethodInfo method = f.FieldType.GetMethod("OnDeserialize", new Type[] { });
                //if (method != null) {
                //    method.Invoke(value, new object[] {});
                //}
            } else {
                Debug.LogFormat("Found serialized yet unserializable field {0}", f.Name);
            }
        }
        return obj;
    }
}