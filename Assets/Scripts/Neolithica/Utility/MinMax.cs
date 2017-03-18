using System;
using UnityEngine;

namespace Neolithica.Utility {
    [Serializable]
    public class MinMax {
        public float Min;
        public float Max;

        public float Lerp(float t) {
            return Mathf.Lerp(Min, Max, t);
        }
    }
}