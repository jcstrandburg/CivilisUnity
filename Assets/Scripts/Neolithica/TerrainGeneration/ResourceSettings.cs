using System;
using AqlaSerializer;

namespace Neolithica.TerrainGeneration {
    [Serializable, SerializableType]
    public class ResourceSettings {
        [SerializableMember(1)] public ResourcePlacementType Type;
        [SerializableMember(2)] public float Frequency;
        [SerializableMember(3)] public float Abundance;
    }
}
