using Neolithica.MonoBehaviours;
using NUnit.Framework;

namespace Neolithica.Test.Editor {
    [TestFixture]
    [Category("Domestication Tests")]
    public class PlantDomesticationTests : NeolithicTest {
        StatManager stats;
        PlantDomesticationManager pdm;

        [SetUp]
        public override void SetUp() {
            base.SetUp();
            stats = MakeDummyStatManager();
            pdm = MakeTestComponent<PlantDomesticationManager>();

            Assert.IsNotNull(pdm.Stats);
            stats.Awake();
            pdm.Start();
        }

        [Test]
        public void TestChangeNotification() {
            pdm.forestGardenThreshold = 1;
            stats.Stat("vegetables-harvested").Add(1);
            Assert.IsTrue(pdm.ForestGardensEnabled);
        }
    }
}
