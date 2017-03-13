using AqlaSerializer;
using UnityEngine;

namespace Tofu.Serialization.Surrogates {
    [SerializableType]
    sealed class QuaternionSurrogate {
        public static implicit operator QuaternionSurrogate(Quaternion value) {
            return new QuaternionSurrogate {
                X = value.x,
                Y = value.y,
                Z = value.z,
                W = value.w
            };
        }

        public static implicit operator Quaternion(QuaternionSurrogate value) {
            return new Quaternion(value.X, value.Y, value.Z, value.W);
        }

        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float W { get; set; }
    }
}