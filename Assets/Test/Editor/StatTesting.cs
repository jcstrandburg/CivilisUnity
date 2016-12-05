﻿using NUnit.Framework;
using System.IO;
using UnityEngine;

[TestFixture]
[Category("Statistic Tests")]
public class StatTests {

    [Test]
    [ExpectedException("System.ArgumentException")]
    public void MonotonicTest() {
        GameObject go = new GameObject();
        StatManager sm = go.AddComponent<StatManager>();

        var stats = new StatProfile[] {
            StatProfile.Make("monotonicStats", false, true),
        };
        sm.SetPersistor(StatManager.DummyPersistor);
        sm.SetStats(stats);
        sm.Stat("monotonicStats").Add(-1);
    }

    /// <summary>
    /// </summary>
    [Test]
    public void GeneralStatTests() {
        GameObject go = new GameObject();
        StatManager sm = go.AddComponent<StatManager>();

        var stats = new StatProfile[] {
            StatProfile.Make("stat1", false, false),
            StatProfile.Make("stat2", false, true),
            StatProfile.Make("stat3", true, false),
        };
        sm.SetPersistor(StatManager.DummyPersistor);
        sm.SetStats(stats);
        sm.Stat("stat2").Add(2);

        Assert.AreEqual(0, sm.Stat("stat1").Value);
        Assert.AreEqual(2, sm.Stat("stat2").Value);
        Assert.AreEqual(0, sm.Stat("stat3").Value);
        Assert.IsNull(sm.Stat("otherstat"));
    }

    [Test]
    public void StreamPersistorTest() {
        var stream = new MemoryStream();
        IStatPersistor p1 = new StreamStatPersistor(stream);
        p1.SetValue("value1", 12);
        p1.SetValue("value2", 14m);
        p1.ExportValues();

        IStatPersistor p2 = new StreamStatPersistor(stream);
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

        GameObject go = new GameObject();
        StatManager sm = go.AddComponent<StatManager>();
        var persist = new StreamStatPersistor(stream);
        
        //test intra-session persistence
        persist.SetValue("stat2", 12m);
        sm.SetPersistor(persist);
        sm.SetStats(stats);
        sm.Stat("stat3").Add(11);
        Assert.AreEqual(12m, sm.Stat("stat2").PersistantValue);
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
        decimal value = 10;

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
