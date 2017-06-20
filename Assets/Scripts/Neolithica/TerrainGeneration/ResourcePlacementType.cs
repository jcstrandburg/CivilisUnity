using System;
using AqlaSerializer;

namespace Neolithica.TerrainGeneration {
    [Serializable, SerializableType]
    public enum ResourcePlacementType {
        None,
        Berries,
        Trees,
        Fish,
        Stone,
        Gold,
        Doodad,
    }
}