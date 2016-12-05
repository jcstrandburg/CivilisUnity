using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[TestFixture]
[Category("Domestication Tests")]
public class PlantDomesticationTests {

    StatManager stats;
    PlantDomesticationManager pdm;
    GameFactory factory;

    [SetUp]
    public void Setup() {
        var go = new GameObject();
        var go2 = new GameObject();

        factory = new GameFactory();
        stats = go.AddComponent<StatManager>();
        pdm = go2.AddComponent<PlantDomesticationManager>();

        factory.statManager = stats;
        factory.InjectObject(go2);

        Assert.IsNotNull(pdm.stats);
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
