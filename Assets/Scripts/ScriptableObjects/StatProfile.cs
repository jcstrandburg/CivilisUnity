using UnityEngine;
using System.Collections;

[CreateAssetMenu]
public class StatProfile : ScriptableObject {
    public string statname;
    public bool persist;
    public bool monotonic;

    public static StatProfile Make(string name, bool persist, bool monotonic) {
        var sp = ScriptableObject.CreateInstance<StatProfile>();
        sp.statname = name;
        sp.persist = persist;
        sp.monotonic = monotonic;
        return sp;
    }
}
