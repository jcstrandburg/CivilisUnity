using NUnit.Framework;
using UnityEngine;

[TestFixture]
[Category("Reservoir Tests")]
public class ReservoirTest : NeolithicTest {

    [Test]
    public void TestReservationsAndWithdrawals() {
        var reservoir = MakeTestComponent<Reservoir>();
        reservoir.resourceType = Resource.Type.Vegetables;
        reservoir.amount = 0;
        reservoir.regenRate = 1;
        reservoir.max = 2;

        reservoir.Regen(1.5f);
        var r1 = reservoir.NewReservation(dummyObject, 1);
        Assert.That(reservoir.GetAvailableContents(), Is.EqualTo(0.5f));
        var r2 = reservoir.NewReservation(dummyObject, 1);
        Assert.That(reservoir.GetAvailableContents(), Is.EqualTo(0.0f));
        reservoir.UpdateReservations();
        Assert.True(r1.Ready);
        Assert.False(r2.Ready);

        reservoir.WithdrawReservation(r1);
        reservoir.Regen(5.0f);
        reservoir.UpdateReservations();

        Assert.That(reservoir.amount, Is.EqualTo(2.0f));
        Assert.That(reservoir.GetAvailableContents(), Is.EqualTo(1.0f));
    }

    [Test]
    public void TestStatIncrementedOnWithdraw() {
        var sm = MakePlainComponent<StatManager>();
        sm.SetPersistor(StatManager.DummyPersistor);
        sm.SetStats(new StatProfile[] { StatProfile.Make("harvested", false, false) });

        var reservoir = MakeTestComponent<Reservoir>();
        reservoir.resourceType = Resource.Type.Vegetables;
        reservoir.amount = 10.0;
        reservoir.statManager = sm;
        reservoir.harvestStat = "harvested";

        var reservoir2 = MakeTestComponent<Reservoir>();
        reservoir2.resourceType = Resource.Type.Vegetables;
        reservoir.amount = 10.0;
        reservoir.statManager = sm;

        //make sure stat is incremented when a reservoir with a harvestStat fills a reservation
        var reservation = reservoir.NewReservation(dummyObject, 2.0);
        reservoir.WithdrawReservation(reservation);
        Assert.That(sm.Stat("harvested").Value, Is.EqualTo(2.0));

        //make sure the reservoir with no harvestStat did not manipulate the stats
        var reservation2 = reservoir2.NewReservation(dummyObject, 2.0);
        reservoir2.WithdrawReservation(reservation2);
        Assert.That(sm.Stat("harvested").Value, Is.EqualTo(2.0));
    }
}
