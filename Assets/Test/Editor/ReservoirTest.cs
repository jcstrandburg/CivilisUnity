using NUnit.Framework;
using UnityEngine;

[TestFixture]
[Category("Reservoir Tests")]
public class ReservoirTest : NeolithicTest {

    [Test]
    public void BasicTest() {
        var go2 = new GameObject();
        var go3 = new GameObject();

        Reservoir reservoir = MakePlainComponent<Reservoir>();
        reservoir.resourceTag = "berries";
        reservoir.amount = 0;
        reservoir.regenRate = 1;
        reservoir.max = 2;

        reservoir.Regen(1.5f);
        ResourceReservation r1 = reservoir.NewReservation(go2, 1);
        Assert.AreEqual(0.5f, reservoir.GetAvailableContents());
        ResourceReservation r2 = reservoir.NewReservation(go3, 1);
        Assert.AreEqual(0.0f, reservoir.GetAvailableContents());
        reservoir.UpdateReservations();
        Assert.True(r1.Ready);
        Assert.False(r2.Ready);

        reservoir.WithdrawReservation(r1);
        reservoir.Regen(5.0f);
        reservoir.UpdateReservations();

        Assert.AreEqual(2.0f, reservoir.amount);
        Assert.AreEqual(1.0f, reservoir.GetAvailableContents());
    }

    [Test]
    public void StatTest() {
        var sm = MakePlainComponent<StatManager>();
        sm.SetPersistor(StatManager.DummyPersistor);
        sm.SetStats(new StatProfile[] { StatProfile.Make("berries-harvested", false, false) });

        var reservoir = MakePlainComponent<Reservoir>();
        reservoir.resourceTag = "berries";
        reservoir.amount = 10.0;
        reservoir.statManager = sm;
        reservoir.harvestStat = "berries-harvested";

        var reservoir2 = MakePlainComponent<Reservoir>();
        reservoir2.resourceTag = "berries";
        reservoir.amount = 10.0;
        reservoir.statManager = sm;

        //make sure stat is incremented when a reservoir with a harvestStat fills a reservation
        var reservation = reservoir.NewReservation(dummyObject, 2.0);
        reservoir.WithdrawReservation(reservation);
        Assert.AreEqual(2.0, sm.Stat("berries-harvested").Value);

        //make sure the reservoir with no harvestStat did not manipulate the stats
        var reservation2 = reservoir2.NewReservation(dummyObject, 2.0);
        reservoir2.WithdrawReservation(reservation2);
        Assert.AreEqual(2.0, sm.Stat("berries-harvested").Value);
    }

}
