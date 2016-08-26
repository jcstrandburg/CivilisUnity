using NUnit.Framework;
using UnityEngine;

[TestFixture]
[Category("Reservoir Tests")]
public class ReservoirTest {

    [Test]
    public void Test() {
        var go = new GameObject();
        var go2 = new GameObject();
        var go3 = new GameObject();

        Reservoir reservoir = go.AddComponent<Reservoir>();
        reservoir.resourceTag = "berries";
        reservoir.amount = 0;
        reservoir.regenRate = 1;
        reservoir.max = 2;

        reservoir.Regen(1.5f);
        reservoir.NewReservation(go2, 1);
        Assert.AreEqual(0.5f, reservoir.GetAvailableContents());
        reservoir.NewReservation(go3, 1);
        Assert.AreEqual(0.0f, reservoir.GetAvailableContents());
        reservoir.UpdateReservations();

        ResourceReservation r1 = go2.GetComponent<ResourceReservation>();
        Assert.True(r1.Ready);
        ResourceReservation r2 = go3.GetComponent<ResourceReservation>();
        Assert.False(r2.Ready);

        reservoir.WithdrawReservation(r1);
        reservoir.Regen(5.0f);
        reservoir.UpdateReservations();

        Assert.AreEqual(2.0f, reservoir.amount);
        Assert.AreEqual(1.0f, reservoir.GetAvailableContents());
    }

}
