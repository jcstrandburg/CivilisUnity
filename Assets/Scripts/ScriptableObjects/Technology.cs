using UnityEngine;
using System.Collections;
using System;

[CreateAssetMenu]
public class Technology : ScriptableObject {
    public string techName;
    public string displayName;
    public string description;
    public string[] requires;
    public float cost;

    public static Technology Make(string name, string displayName, string desc, string[] requires, float cost) {
        var t = ScriptableObject.CreateInstance<Technology>();
        t.techName = name;
        t.displayName = displayName;
        t.description = desc;
        t.requires = requires;
        t.cost = cost;
        return t;
    }
}
