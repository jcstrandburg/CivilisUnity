using NUnit.Framework;
using System.IO;
using UnityEngine;

[TestFixture]
[Category("Statistic Tests")]
public class StatTests : NeolithicTest {

    [Test]
    [ExpectedException("System.ArgumentException")]
    public void MonotonicTest() {
        var stats = new StatProfile[] {
            StatProfile.Make("monotonicStat", false, true),
        };
        var statManager = MakeDummyStatManager();
        statManager.SetStats(stats);
        statManager.Stat("monotonicStat").Add(-1);
    }

    /// <summary>
    /// </summary>
    [Test]
    public void GeneralStatTests() {
        var stats = new StatProfile[] {
            StatProfile.Make("stat1", false, false),
            StatProfile.Make("stat2", false, true),
            StatProfile.Make("stat3", true, false),
        };
        var statManager = MakeDummyStatManager();
        statManager.SetStats(stats);
        statManager.Stat("stat2").Add(2);

        Assert.AreEqual(0, statManager.Stat("stat1").Value);
        Assert.AreEqual(2, statManager.Stat("stat2").Value);
        Assert.AreEqual(0, statManager.Stat("stat3").Value);
        Assert.IsNull(statManager.Stat("otherstat"));
    }

    [Test]
    public void StreamPersistorTest() {
        var stream = new MemoryStream();
        StatPersistor p1 = new StreamStatPersistor(stream);
        p1.SetValue("value1", 12);
        p1.SetValue("value2", 14m);
        p1.ExportValues();

        StatPersistor p2 = new StreamStatPersistor(stream);
        p2.ImportValues();
        Assert.AreEqual(12m, p2.GetValue("value1"));
        Assert.AreEqual(14m, p2.GetValue("value2"));
    }

    [Test]
    public void TestManagerPersistence() {
        var stats = new StatProfile[] {
            StatProfile.Make("stat1", false, false),
            StatProfile.Make("stat2", true, true),
            StatProfile.Make("stat3", true, false),
        };
        var stream = new MemoryStream();
        var persist = new StreamStatPersistor(stream);
        var statManager = MakeTestComponent<StatManager>();

        //test intra-session persistence
        persist.SetValue("stat2", 12m);
        statManager.SetPersistor(persist);
        statManager.SetStats(stats);
        statManager.Stat("stat3").Add(11);
        Assert.AreEqual(12m, statManager.Stat("stat2").PersistantValue);
        Assert.AreEqual(11m, persist.GetValue("stat3"));

        //test exporting persistence
        persist.ExportValues();
        var go2 = new GameObject();
        var sm2 = go2.AddComponent<StatManager>();
        sm2.SetPersistor(new StreamStatPersistor(stream));
        sm2.SetStats(stats);
        Assert.AreEqual(12m, sm2.Stat("stat2").PersistantValue);
        Assert.AreEqual(11m, sm2.Stat("stat3").PersistantValue);
    }

    [Test]
    public void TestOnChangeNotification() {
        GameStat testStat = new GameStat("teststat", false, false);
        int invocations = 0;
        decimal value = -1;

        testStat.OnChange += (stat) => {
            invocations++;
            value = stat.Value;
            Assert.AreSame(testStat, stat);
        };

        testStat.Add(10);
        testStat.Add(0);

        Assert.AreEqual(1, invocations);
        Assert.AreEqual(10, value);
    }
}
