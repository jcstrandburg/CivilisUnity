using System;

namespace Neolithica.TerrainGeneration {
    [Serializable]
    public class EroderSettings {
        public float Gravity = 2.5f;
        public float Erosion = 0.45f;
        public float Deposition = 0.175f;
        public float Evaporation = 0.03f;
        public float Inertia = 0.00001f;
        public float MinSlope = 0.075f;
        public float Capacity = 10;
        public float PitDigging = 2.0f;
        public float ErosionRadius = 2.0f;
    }
}
