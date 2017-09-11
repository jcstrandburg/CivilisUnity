using System;
using System.Collections.Generic;
using AqlaSerializer;
using Neolithica.TerrainGeneration;

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
        [SerializableMember(15)] public float waterLevel = 0.25f;
    }

    [Serializable, SerializableType]
    public class NewGameSettings {
        [SerializableMember(1)] public float seed;

        [SerializableMember(2)] public List<ResourceSettings> ResourceSettings = new List<ResourceSettings> {
            new ResourceSettings {Type = ResourcePlacementType.Berries, Frequency = 4.0f, Abundance = 0.3f},
            new ResourceSettings {Type = ResourcePlacementType.Trees, Frequency = 1.5f, Abundance = 0.5f},
            new ResourceSettings {Type = ResourcePlacementType.Fish, Frequency = 5.0f, Abundance = 0.2f},
            new ResourceSettings {Type = ResourcePlacementType.Gold, Frequency = 2.0f, Abundance = 0.2f},
            new ResourceSettings {Type = ResourcePlacementType.Stone, Frequency = 2.0f, Abundance = 0.2f},
            new ResourceSettings {Type = ResourcePlacementType.Doodad, Frequency = 10.0f, Abundance = 0.15f},
        };
    }
}
