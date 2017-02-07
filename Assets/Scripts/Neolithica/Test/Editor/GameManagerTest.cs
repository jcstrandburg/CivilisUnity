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
                new ResourceProfile(Resource.Type.Meat, 1),
                new ResourceProfile(Resource.Type.Vegetables, 1),
                new ResourceProfile(Resource.Type.Fish, 1),
            };
            Assert.AreEqual(6.0f, network.CalcFoodValue(resources));

            resources = new ResourceProfile[] {
                new ResourceProfile(Resource.Type.Meat, 2.0),
                new ResourceProfile(Resource.Type.Vegetables, 1),
                new ResourceProfile(Resource.Type.Fish, 1),
            };
            Assert.AreEqual(7.0f, network.CalcFoodValue(resources));

            resources = new ResourceProfile[] {
                new ResourceProfile(Resource.Type.Meat, 0.5),
                new ResourceProfile(Resource.Type.Vegetables, 0.75),
                new ResourceProfile(Resource.Type.Fish, 1),
            };
            Assert.AreEqual(4.0f, network.CalcFoodValue(resources));

            resources = new ResourceProfile[] {
                new ResourceProfile(Resource.Type.Vegetables, 2.0),
                new ResourceProfile(Resource.Type.Fish, 1),
            };
            Assert.AreEqual(4.0f, network.CalcFoodValue(resources));

            resources = new ResourceProfile[] {
                new ResourceProfile(Resource.Type.Meat, 1),
                new ResourceProfile(Resource.Type.Fish, 0.5),
            };
            Assert.AreEqual(2.0f, network.CalcFoodValue(resources));

            resources = new ResourceProfile[] {
                new ResourceProfile(Resource.Type.Meat, 1.5),
            };
            Assert.AreEqual(1.5f, network.CalcFoodValue(resources));

            resources = new ResourceProfile[] {
                new ResourceProfile(Resource.Type.Vegetables, 1),
            };
            Assert.AreEqual(1, network.CalcFoodValue(resources));
        }
    }
}
