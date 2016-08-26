using NUnit.Framework;
using UnityEngine;

[TestFixture]
[Category("Reservoir Tests")]
public class GameManagerTests {


    [Test]
    public void TestFoodValue() {
        var go = new GameObject();
        var netowrk = go.AddComponent<LogisticsNetwork>();
        ResourceProfile[] resources;

        resources = new ResourceProfile[] {
            new ResourceProfile("meat", 1),
            new ResourceProfile("vegetables", 1),
            new ResourceProfile("fish", 1),
        };
        Assert.AreEqual(6.0f, netowrk.CalcFoodValue(resources));

        resources = new ResourceProfile[] {
            new ResourceProfile("meat", 2.0),
            new ResourceProfile("vegetables", 1),
            new ResourceProfile("fish", 1),
        };
        Assert.AreEqual(7.0f, netowrk.CalcFoodValue(resources));

        resources = new ResourceProfile[] {
            new ResourceProfile("meat", 0.5),
            new ResourceProfile("vegetables", 0.75),
            new ResourceProfile("fish", 1),
        };
        Assert.AreEqual(4.0f, netowrk.CalcFoodValue(resources));

        resources = new ResourceProfile[] {
            new ResourceProfile("vegetables", 2.0),
            new ResourceProfile("fish", 1),
        };
        Assert.AreEqual(4.0f, netowrk.CalcFoodValue(resources));

        resources = new ResourceProfile[] {
            new ResourceProfile("meat", 1),
            new ResourceProfile("fish", 0.5),
        };
        Assert.AreEqual(2.0f, netowrk.CalcFoodValue(resources));

        resources = new ResourceProfile[] {
            new ResourceProfile("meat", 1.5),
        };
        Assert.AreEqual(1.5f, netowrk.CalcFoodValue(resources));

        resources = new ResourceProfile[] {
            new ResourceProfile("vegetables", 1),
        };
        Assert.AreEqual(1, netowrk.CalcFoodValue(resources));
    }
}
