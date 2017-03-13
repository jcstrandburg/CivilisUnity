using AqlaSerializer;
using UnityEngine;

namespace Tofu.Serialization.Surrogates {
    [SerializableType]
    public class Vector3Surrogate {
        public static implicit operator Vector3Surrogate(Vector3 value) {
            return new Vector3Surrogate {
                X = value.x,
                Y = value.y,
                Z = value.z
            };
        }

        public static implicit operator Vector3(Vector3Surrogate value) {
            return new Vector3(value.X, value.Y, value.Z);
        }

        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
    }
}