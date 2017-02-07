using System;
using Neolithica.Serialization.Attributes;
using UnityEngine;

namespace Neolithica.Test.NonMono {
    [CustomSerialize]
    [Serializable]
    public class TestContainer {
        public GameObject z;
        public Vector3 v;
    }
}
