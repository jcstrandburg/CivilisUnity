using UnityEngine;
using System;

[Serializable]
public class Settings {
    public int x;
    public float y;
    public string z;
}


public class SettingsMenu : MonoBehaviour {
    public Settings settings;

    public void Awake() {
        DataBindingSource dbs = GetComponent<DataBindingSource>();
        dbs.AddBinding( "y", 
                        () => settings.y, 
                        (object val) => settings.y = Convert.ToSingle(val));
    }
}
