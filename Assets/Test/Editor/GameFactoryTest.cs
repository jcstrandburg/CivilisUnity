using UnityEngine;
using UnityEditor;
using NUnit.Framework;

public class GameFactoryTest {

	[Test]
	public void TestLogisticsInjection()
	{
        var factory = new GameFactory();
        var managerObject = new GameObject();
        var logisticsManager = managerObject.AddComponent<LogisticsManager>();
        var nodeObject = new GameObject();
        var logisticsnode = nodeObject.AddComponent<LogisticsNode>();

        factory.InjectObject(nodeObject);
        Assert.AreSame(logisticsManager, logisticsnode.logisticsManager);
	}
}
