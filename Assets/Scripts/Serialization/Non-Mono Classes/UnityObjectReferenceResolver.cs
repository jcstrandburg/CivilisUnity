using UnityEngine;
using System.Reflection;
using System.Collections.Generic;

public class UnityObjectReferenceResolver : IReferenceResolver {
    private FieldInfo field;
    object owner;
    UnityObjectReference reference;
    Dictionary<string, ObjectIdentifier> source;

    public UnityObjectReferenceResolver(FieldInfo f, object o, UnityObjectReference r, SaveLoadContext context) {
        if (f == null) {
            Debug.Log("FieldInfo is null");
            Debug.Log(r.typeName);
            Debug.Log(r.refName);
            Debug.Log(r.refID);
        }
        field = f;
        owner = o;
        reference = r;
        source = context.oidReferences;
    }
     
	public void Resolve() {
        if (source.ContainsKey(reference.refID)) {
            //Debug.Log("Found id " + r.refID + " for " + fieldName);
            ObjectIdentifier oid = source[reference.refID];
            GameObject go = oid.gameObject;

            if (reference.typeName == "GameObject") {
                field.SetValue(owner, go);
            }
            else {
                var component = oid.GetComponent(reference.typeName);
                if (component != null) {
                    field.SetValue(owner, component);
                } else {
                    Debug.Log("Unable to resolve reference to " + reference.typeName + " from " + reference.refName);
                }
            }
        }
        else {
            Debug.Log("Unable to resolve reference for id " + reference.refID);
        }
    }
}
