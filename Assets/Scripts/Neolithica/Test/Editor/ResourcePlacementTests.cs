using Neolithica.TerrainGeneration;
using NUnit.Framework;

namespace Neolithica.Test.Editor {
    [TestFixture]
    [Category("Terrain Generation")]
    public class ResourcePlacementTests {
        [TestCase(0.5f, 0.4f, 0.6f, 0.5f)]
        [TestCase(0.5f, 0.6f, 0.4f, 0.5f)]
        [TestCase(0.25f, 0.2f, 0.4f, 0.25f)]
        [TestCase(0.25f, 0.4f, 0.2f, 0.75f)]
        [TestCase(0.25f, 0.6f, 0.8f, 0.0f)]
        [TestCase(0.9f, 0.6f, 0.8f, 0.0f)]
        public void FitnessHelpersGradient(float height, float zeroPoint, float onePoint, float expectedValue) {
            Assert.AreEqual(expectedValue, FitnessHelpers.Gradient(height, zeroPoint, onePoint), 0.01f);
        }

        [TestCase(0.175f, 0.1f, 0.2f, 0.4f, 0.75f)]
        [TestCase(0.3f, 0.1f, 0.2f, 0.4f, 0.5f)]
        [TestCase(0.35f, 0.1f, 0.2f, 0.4f, 0.25f)]
        [TestCase(0.05f, 0.1f, 0.2f, 0.4f, 0.0f)]
        [TestCase(0.5f, 0.1f, 0.2f, 0.4f, 0.0f)]
        public void FitnessHelpersPivotGradient(float height, float end1, float pivot, float end2, float expectedValue) {
            Assert.AreEqual(expectedValue, FitnessHelpers.PivotGradient(height, end1, pivot, end2), 0.01f);
        }
    }
}
