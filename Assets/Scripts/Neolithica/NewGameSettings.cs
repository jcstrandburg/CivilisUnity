using System;

namespace Neolithica {
    [Serializable]
    public sealed class TerrainSettings {
        public bool cosineTerrain = true;
        public float mountainExponent = 2.0f;
        public float mountainFrequency = 6.0f;
        public float mountainHeight = 2.0f;
        public float mountainSharpness = 1.0f;
        public float mountainFalloff = 2.0f;
        public float bumpFrequency = 15.0f;
        public float bumpHeight = 0.02f;
        public float riverFrequency = 1.0f;
        public float riverHeight = 2.0f;
        public float riverExponent = 1.4f;
        public float hillFrequency = 3.0f;
        public float hillHeight = 0.12f;
        public float hillBaseOffset = -0.25f;
    }

    [Serializable]
    public class NewGameSettings {
        public float seed;
        public float treeThinning = 150.0f;
        public float treeMultiplier = 1/100.0f;
        public float berryMultiplier = 1/150.0f;
        public float stoneRate = 0.17f;
        public float fishRate = 0.2f;
        public float doodadRate = 0.35f;
    }
}