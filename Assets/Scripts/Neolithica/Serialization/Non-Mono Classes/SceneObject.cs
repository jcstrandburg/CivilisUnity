using System;
using System.Collections.Generic;
using UnityEngine;

namespace Neolithica.Serialization {
    /// <summary>
    /// Part of the SerializeHelper package by Cherno.
    /// http://forum.unity3d.com/threads/serializehelper-free-save-and-load-utility-de-serialize-all-objects-in-your-scene.338148/
    /// This class holds is meant to hold all the data of a GameObject in the scene which has an ObjectIdentifier component. 
    /// The values from the OI component are mirrored here, along with misc. stuff like the activation state of the gameObect (bool active), and of course it's components.
    /// </summary>
    [System.Serializable]
    public class SceneObject {
	
        public string name;
        public string prefabName;
        public string id;
        public string idParent;

        public bool active;
        public Vector3 position;
        public Vector3 localScale;
        public Quaternion rotation;

        public List<ObjectComponent> objectComponents = new List<ObjectComponent>();

        [NonSerialized]
        public GameObject objectInstance;
    }
}



