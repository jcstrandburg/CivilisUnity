using System.Collections.Generic;
using AqlaSerializer;
using UnityEngine;

namespace Tofu.Serialization {
    [SerializableType]
    public class SaveGame {
        [SerializableMember(1)]
        public List<GameObject> GameObjects { get; set; }
        [SerializableMember(2)]
        public List<MonoBehaviour> Behaviours { get; set; }
    }
}