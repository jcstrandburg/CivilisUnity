using Neolithica.MonoBehaviours;
using Neolithica.MonoBehaviours.Logistics;
using NUnit.Framework;

namespace Neolithica.Test.Editor {
    [TestFixture]
    [Category("Reservoir Tests")]
    public class GameManagerTests : NeolithicTest {

        [Test]
        public void TestFoodValue() {
            var network = MakePlainComponent<LogisticsNetwork>();
            ResourceProfile[] resources;

            resources = new ResourceProfile[] {
                new ResourceProfile(ResourceKind.Meat, 1),
                new ResourceProfile(ResourceKind.Vegetables, 1),
                new ResourceProfile(ResourceKind.Fish, 1),
            };
            Assert.AreEqual(6.0f, network.CalcFoodValue(resources));

            resources = new ResourceProfile[] {
                new ResourceProfile(ResourceKind.Meat, 2.0),
                new ResourceProfile(ResourceKind.Vegetables, 1),
                new ResourceProfile(ResourceKind.Fish, 1),
            };
            Assert.AreEqual(7.0f, network.CalcFoodValue(resources));

            resources = new ResourceProfile[] {
                new ResourceProfile(ResourceKind.Meat, 0.5),
                new ResourceProfile(ResourceKind.Vegetables, 0.75),
                new ResourceProfile(ResourceKind.Fish, 1),
            };
            Assert.AreEqual(4.0f, network.CalcFoodValue(resources));

            resources = new ResourceProfile[] {
                new ResourceProfile(ResourceKind.Vegetables, 2.0),
                new ResourceProfile(ResourceKind.Fish, 1),
            };
            Assert.AreEqual(4.0f, network.CalcFoodValue(resources));

            resources = new ResourceProfile[] {
                new ResourceProfile(ResourceKind.Meat, 1),
                new ResourceProfile(ResourceKind.Fish, 0.5),
            };
            Assert.AreEqual(2.0f, network.CalcFoodValue(resources));

            resources = new ResourceProfile[] {
                new ResourceProfile(ResourceKind.Meat, 1.5),
            };
            Assert.AreEqual(1.5f, network.CalcFoodValue(resources));

            resources = new ResourceProfile[] {
                new ResourceProfile(ResourceKind.Vegetables, 1),
            };
            Assert.AreEqual(1, network.CalcFoodValue(resources));
        }
    }
}
