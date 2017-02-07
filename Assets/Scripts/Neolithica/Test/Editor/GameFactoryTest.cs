using Neolithica.MonoBehaviours.Logistics;
using NUnit.Framework;

namespace Neolithica.Test.Editor {
    public class GameFactoryTest : NeolithicTest {

        [Test]
        public void TestLogisticsInjection()
        {
            var logisticsManager = MakeTestComponent<LogisticsManager>();
            var logisticsnode = MakeTestComponent<LogisticsNode>();
            factory.InjectObject(logisticsnode);
            Assert.AreSame(logisticsManager, logisticsnode.LogisticsLogisticsManager);
        }
    }
}
