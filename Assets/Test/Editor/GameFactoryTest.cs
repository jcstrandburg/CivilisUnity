using UnityEngine;
using UnityEditor;
using NUnit.Framework;

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
