using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu]
public class ActionProfile : ScriptableObject {
    public string[] targetActions;
    public string[] abilities;

    public static ActionProfile Make(string[] targetActions, string[] abilities) {
        var ap = ScriptableObject.CreateInstance<ActionProfile>();
        ap.targetActions = targetActions;
        ap.abilities = abilities;
        return ap;
    }
}
