using System;
using AqlaSerializer;

namespace Neolithica {
    [Serializable, SerializableType]
    public sealed class TerrainSettings {
        [SerializableMember(1)] public bool cosineTerrain = true;
        [SerializableMember(2)] public float mountainExponent = 2.0f;
        [SerializableMember(3)] public float mountainFrequency = 6.0f;
        [SerializableMember(4)] public float mountainHeight = 2.0f;
        [SerializableMember(5)] public float mountainSharpness = 1.0f;
        [SerializableMember(6)] public float mountainFalloff = 2.0f;
        [SerializableMember(7)] public float bumpFrequency = 15.0f;
        [SerializableMember(8)] public float bumpHeight = 0.02f;
        [SerializableMember(9)] public float riverFrequency = 1.0f;
        [SerializableMember(10)] public float riverHeight = 2.0f;
        [SerializableMember(11)] public float riverExponent = 1.4f;
        [SerializableMember(12)] public float hillFrequency = 3.0f;
        [SerializableMember(13)] public float hillHeight = 0.12f;
        [SerializableMember(14)] public float hillBaseOffset = -0.25f;
    }

    [Serializable, SerializableType]
    public class NewGameSettings {
        [SerializableMember(1)] public float seed;
        [SerializableMember(2)] public float treeThinning = 150.0f;
        [SerializableMember(3)] public float treeMultiplier = 1/100.0f;
        [SerializableMember(4)] public float berryMultiplier = 1/150.0f;
        [SerializableMember(5)] public float stoneRate = 0.17f;
        [SerializableMember(6)] public float fishRate = 0.2f;
        [SerializableMember(7)] public float doodadRate = 0.35f;
    }
}
