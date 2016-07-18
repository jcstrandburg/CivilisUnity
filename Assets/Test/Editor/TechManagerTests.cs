using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using NSubstitute;
using System.Collections.Generic;
using System;
using System.Linq;

[TestFixture]
[Category("Techmanager Tests")]
public class TechManagerTests {

    /// <summary>
    /// Helper function to build techs for testing purposes
    /// </summary>
    /// <param name="name"></param>
    /// <param name="displayName"></param>
    /// <param name="desc"></param>
    /// <param name="requires"></param>
    /// <param name="cost"></param>
    /// <returns></returns>
    private Technology MakeTech(string name, string displayName, string desc, string[] requires, float cost) {
        Technology t = new Technology();
        t.name = name;
        t.displayName = displayName;
        t.description = desc;
        t.requires = requires;
        t.cost = cost;
        return t;
    }

    /// <summary>
    /// Tests elligibility and prereqs functionality
    /// </summary>
    [Test]
    public void Test1() {
        Technology[] techs = new Technology[] {
            MakeTech("0", "0", "", new string[] {}, 1.0f),
            MakeTech("1", "1", "", new string[] {}, 1.0f),
            MakeTech("2", "2", "", new string[] {}, 3.0f),
            MakeTech("3", "3", "", new string[] {"0"}, 1.0f),
            MakeTech("4", "4", "", new string[] {"0","1"}, 1.0f),
            MakeTech("5", "5", "", new string[] {"1"}, 1.0f),
            MakeTech("6", "6", "", new string[] {"2"}, 1.0f),
            MakeTech("7", "7", "", new string[] {"4","1"}, 1.0f),
        };
        TechManager tm = new TechManager();
        Technology[] eligibles;
        tm.LoadArray(techs);

        eligibles = tm.GetEligibleTechs();
        Assert.IsTrue(tm.PrereqsMet(techs[0]));
        Assert.IsTrue(tm.PrereqsMet(techs[1]));
        Assert.IsTrue(tm.PrereqsMet(techs[2]));
        Assert.IsTrue(!tm.PrereqsMet(techs[3]));
        Assert.IsTrue(!tm.PrereqsMet(techs[4]));
        Assert.IsTrue(!tm.PrereqsMet(techs[5]));
        Assert.IsTrue(!tm.PrereqsMet(techs[6]));
        Assert.IsTrue(!tm.PrereqsMet(techs[7]));
        Assert.IsTrue(eligibles.Contains<Technology>(techs[0]));
        Assert.IsTrue(eligibles.Contains<Technology>(techs[1]));
        Assert.IsTrue(eligibles.Contains<Technology>(techs[2]));
        Assert.IsTrue(!eligibles.Contains<Technology>(techs[3]));
        Assert.IsTrue(!eligibles.Contains<Technology>(techs[4]));
        Assert.IsTrue(!eligibles.Contains<Technology>(techs[5]));
        Assert.IsTrue(!eligibles.Contains<Technology>(techs[6]));
        Assert.IsTrue(!eligibles.Contains<Technology>(techs[7]));

        Assert.AreEqual(1.0f, tm.BuyTech("0"));
        Assert.AreEqual(0.0f, tm.BuyTech("0"));
        Assert.AreEqual(3.0f, tm.BuyTech("2"));

        eligibles = tm.GetEligibleTechs();
        Assert.IsTrue(tm.PrereqsMet(techs[0]));
        Assert.IsTrue(tm.PrereqsMet(techs[1]));
        Assert.IsTrue(tm.PrereqsMet(techs[2]));
        Assert.IsTrue(tm.PrereqsMet(techs[3]));
        Assert.IsTrue(!tm.PrereqsMet(techs[4]));
        Assert.IsTrue(!tm.PrereqsMet(techs[5]));
        Assert.IsTrue(tm.PrereqsMet(techs[6]));
        Assert.IsTrue(!tm.PrereqsMet(techs[7]));
        Assert.IsTrue(!eligibles.Contains<Technology>(techs[0]));
        Assert.IsTrue(eligibles.Contains<Technology>(techs[1]));
        Assert.IsTrue(!eligibles.Contains<Technology>(techs[2]));
        Assert.IsTrue(eligibles.Contains<Technology>(techs[3]));
        Assert.IsTrue(!eligibles.Contains<Technology>(techs[4]));
        Assert.IsTrue(!eligibles.Contains<Technology>(techs[5]));
        Assert.IsTrue(eligibles.Contains<Technology>(techs[6]));
        Assert.IsTrue(!eligibles.Contains<Technology>(techs[7]));

        tm.BuyTech("1");
        eligibles = tm.GetEligibleTechs();
        Assert.IsTrue(tm.PrereqsMet(techs[4]));
        Assert.IsTrue(!tm.PrereqsMet(techs[7]));
        Assert.IsTrue(eligibles.Contains<Technology>(techs[4]));
        Assert.IsTrue(!eligibles.Contains<Technology>(techs[7]));

        tm.BuyTech("4");
        eligibles = tm.GetEligibleTechs();
        Assert.IsTrue(tm.PrereqsMet(techs[7]));
        Assert.IsTrue(eligibles.Contains<Technology>(techs[7]));
    }
}
