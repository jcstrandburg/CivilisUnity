using UnityEngine;

namespace Neolithica.TerrainGeneration {
    /// <summary>
    /// Static methods for resource placement in map generation. All methods should return a value in the range [0.0f, 1.0f]
    /// </summary>
    public static class FitnessHelpers {
        /// <summary>
        /// Returns 1.0 at <paramref name="onePoint"/>, fading towards zero at <paramref name="zeroPoint"/>.
        /// Any <paramref name="height"/> outside of the given range returns 0.
        /// </summary>
        public static float Gradient(float height, float zeroPoint, float onePoint) {
            float gradientValue = (height - zeroPoint) / (onePoint - zeroPoint);

            return gradientValue <= 1.0f
                ? Mathf.Max(0.0f, gradientValue)
                : 0.0f;
        }

        /// <summary>
        /// Returns 1.0 at <paramref name="pivot"/>, fading towards zero at <paramref name="end1"/> and <paramref name="end2"/>
        /// </summary>
        public static float PivotGradient(float height, float end1, float pivot, float end2) {
            return height > pivot
                ? Gradient(height, end2, pivot)
                : Gradient(height, end1, pivot);
        }

        /// <summary>
        /// Returns 1.0 for <paramref name="height"/> below <paramref name="waterHeight"/>, else 0.0
        /// </summary>
        public static float Water(float height, float waterHeight) {
            return height < waterHeight ? 1.0f : 0.0f;
        }

        /// <summary>
        /// Returns 1.0 for <paramref name="height"/> above <paramref name="waterHeight"/>, else 0.0
        /// </summary>
        public static float DryLand(float height, float waterHeight) {
            return height > waterHeight ? 1.0f : 0.0f;
        }
    }
}
