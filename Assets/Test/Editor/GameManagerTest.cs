using NUnit.Framework;
using UnityEngine;

[TestFixture]
[Category("Reservoir Tests")]
public class GameManagerTests {


    [Test]
    public void TestFoodValue() {
        var go = new GameObject();
        var gc = go.AddComponent<GameController>();
        ResourceProfile[] resources;

        resources = new ResourceProfile[] {
            new ResourceProfile("meat", 1.0f),
            new ResourceProfile("vegetables", 1.0f),
            new ResourceProfile("fish", 1.0f),
        };
        Assert.AreEqual(6.0f, gc.CalcFoodValue(resources));

        resources = new ResourceProfile[] {
            new ResourceProfile("meat", 2.0f),
            new ResourceProfile("vegetables", 1.0f),
            new ResourceProfile("fish", 1.0f),
        };
        Assert.AreEqual(7.0f, gc.CalcFoodValue(resources));

        resources = new ResourceProfile[] {
            new ResourceProfile("meat", 0.5f),
            new ResourceProfile("vegetables", 0.75f),
            new ResourceProfile("fish", 1.0f),
        };
        Assert.AreEqual(4.0f, gc.CalcFoodValue(resources));

        resources = new ResourceProfile[] {
            new ResourceProfile("vegetables", 2.0f),
            new ResourceProfile("fish", 1.0f),
        };
        Assert.AreEqual(4.0f, gc.CalcFoodValue(resources));

        resources = new ResourceProfile[] {
            new ResourceProfile("meat", 1.0f),
            new ResourceProfile("fish", 0.5f),
        };
        Assert.AreEqual(2.0f, gc.CalcFoodValue(resources));

        resources = new ResourceProfile[] {
            new ResourceProfile("meat", 1.5f),
        };
        Assert.AreEqual(1.5f, gc.CalcFoodValue(resources));

        resources = new ResourceProfile[] {
            new ResourceProfile("vegetables", 1.0f),
        };
        Assert.AreEqual(1.0f, gc.CalcFoodValue(resources));
    }
}
