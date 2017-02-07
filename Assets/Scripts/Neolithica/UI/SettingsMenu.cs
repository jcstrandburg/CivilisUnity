using System;
using UnityEngine;

namespace Neolithica.UI {
    public class SettingsMenu : MonoBehaviour {
        [Serializable]
        public class Settings {
            public int x;
            public float y;
            public string z;
        }

        public Settings settings;

        public void Awake() {
            DataBindingSource dbs = GetComponent<DataBindingSource>();
            dbs.AddBinding( "y", 
                () => settings.y, 
                (object val) => settings.y = Convert.ToSingle(val));
        }
    }
}
