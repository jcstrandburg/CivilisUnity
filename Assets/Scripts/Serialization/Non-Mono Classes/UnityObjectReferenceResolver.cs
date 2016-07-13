using UnityEngine;
using System.Reflection;
using System.Collections.Generic;

public class UnityObjectReferenceResolver : IReferenceResolver {
    private FieldInfo field;
    object owner;
    UnityObjectReference reference;
    Dictionary<string, ObjectIdentifier> source;

    public UnityObjectReferenceResolver(FieldInfo f, object o, UnityObjectReference r, SaveLoadContext context) {
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
                field.SetValue(owner, oid.GetComponent(reference.typeName));
            }
        }
        else {
            Debug.Log("Unable to resolve reference for id " + reference.refID);
        }
    }
}
