using System;

namespace Neolithica {
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