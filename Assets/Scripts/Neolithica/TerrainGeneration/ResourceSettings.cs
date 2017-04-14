using System;

namespace Neolithica.TerrainGeneration {
    [Serializable]
    public class ResourceSettings {
        public ResourcePlacementType Type;
        public float Frequency;
        public float Abundance;
    }
}
