using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu]
public class ActionProfile : ScriptableObject {
    public CommandType[] targetActions;
    public CommandType[] abilities;

    public static ActionProfile Make(CommandType[] targetActions, CommandType[] abilities) {
        var ap = ScriptableObject.CreateInstance<ActionProfile>();
        ap.targetActions = targetActions;
        ap.abilities = abilities;
        return ap;
    }
}
