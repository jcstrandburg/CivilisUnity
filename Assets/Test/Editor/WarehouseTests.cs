using NUnit.Framework;
using UnityEngine;

[TestFixture]
[Category("Warehouse Tests")]
public class WarehouseTests {

    /// <summary>
    /// Tests that when you set resource contents with profiles missing for 
    /// items present in the resource limits, the holes in the contents get 
    /// filled in to allow those resources to be deposited
    /// (This test sets limits before setting contents)
    /// </summary>
    [Test]
    public void TestContentsMissingFromLimits1() {
        var go = new GameObject();
        Warehouse w = go.AddComponent<Warehouse>();

        var resourceLimits = new ResourceProfile[] {
            new ResourceProfile("wood", 2.0f),
            new ResourceProfile("fish", 2.0f),
            new ResourceProfile("gold", 2.0f),
            new ResourceProfile("stone", 2.0f),
        };
        var resourceContents = new ResourceProfile[] {
            new ResourceProfile("fish", 2.0f),
            new ResourceProfile("gold", 2.0f),
        };

        w.SetLimits(resourceLimits);
        w.SetContents(resourceContents);

        Assert.False(w.ReserveStorage(go, "stone", 3.0f));
        Assert.IsNull(go.GetComponent<StorageReservation>());
        Assert.True(w.ReserveStorage(go, "stone", 1.0f));
        var res = go.GetComponent<StorageReservation>();
        Assert.IsNotNull(res);

        Assert.True(res.Ready);
        w.DepositReservation(res);
        Assert.True(res.Released);
        Assert.AreEqual(1.0f, w.GetAvailableContents("stone"));

        Assert.False(w.ReserveContents(go, "wood", 1.0f));
        Assert.IsNull(go.GetComponent<ResourceReservation>());
        Assert.True(w.ReserveContents(go, "stone", 1.0f));
        var res2 = go.GetComponent<ResourceReservation>();
        Assert.IsNotNull(res2);
        w.WithdrawReservation(res2);
        Assert.True(res2.Released);
    }

    /// <summary>
    /// Tests that when you set resource contents with profiles missing for 
    /// items present in the resource limits, the holes in the contents get 
    /// filled in to allow those resources to be deposited
    /// (This test sets contents before setting limits)
    /// </summary>
    [Test]
    public void TestContentsMissingFromLimits2() {
        var go = new GameObject();
        Warehouse w = go.AddComponent<Warehouse>();

        var resourceLimits = new ResourceProfile[] {
            new ResourceProfile("wood", 2.0f),
            new ResourceProfile("fish", 2.0f),
            new ResourceProfile("gold", 2.0f),
            new ResourceProfile("stone", 2.0f),
        };
        var resourceContents = new ResourceProfile[] {
            new ResourceProfile("fish", 2.0f),
            new ResourceProfile("gold", 2.0f),
        };

        w.SetContents(resourceContents);
        w.SetLimits(resourceLimits);

        Assert.False(w.ReserveStorage(go, "stone", 3.0f));
        Assert.IsNull(go.GetComponent<StorageReservation>());
        Assert.True(w.ReserveStorage(go, "stone", 1.0f));
        var res = go.GetComponent<StorageReservation>();
        Assert.IsNotNull(res);

        Assert.True(res.Ready);
        w.DepositReservation(res);
        Assert.True(res.Released);
        Assert.AreEqual(1.0f, w.GetAvailableContents("stone"));

        Assert.False(w.ReserveContents(go, "wood", 1.0f));
        Assert.IsNull(go.GetComponent<ResourceReservation>());
        Assert.True(w.ReserveContents(go, "stone", 1.0f));
        var res2 = go.GetComponent<ResourceReservation>();
        Assert.IsNotNull(res2);
        w.WithdrawReservation(res2);
        Assert.True(res2.Released);
    }

    [Test]
    public void TestContentsAndSpaceAvailability() {
        var go = new GameObject();
        var go2 = new GameObject();
        Warehouse w = go.AddComponent<Warehouse>();
        w.SetLimits(new ResourceProfile[] {
            new ResourceProfile("stone", 10.0f),
        });

        Assert.AreEqual(0.0f, w.GetTotalContents("stone"));
        Assert.AreEqual(0.0f, w.GetReservedStorage("stone"));
        Assert.AreEqual(10.0f, w.GetAvailableStorage("stone"));

        Assert.True(w.ReserveStorage(go2, "stone", 2.0f));
        var res = go2.GetComponent<StorageReservation>();
        Assert.IsNotNull(res);
        Assert.AreEqual(8.0f, w.GetAvailableStorage("stone"));
        Assert.AreEqual(0.0f, w.GetAvailableContents("stone"));
        w.DepositReservation(res);
        Assert.True(res.Released);
        Assert.AreEqual(2.0f, w.GetAvailableContents("stone"));
        Assert.AreEqual(2.0f, w.GetTotalContents("stone"));

        Assert.True(w.ReserveContents(go2, "stone", 1.0f));
        var res2 = go2.GetComponent<ResourceReservation>();
        Assert.IsNotNull(res2);
        Assert.AreEqual(1.0f, w.GetReservedContents("stone"));
        Assert.AreEqual(1.0f, w.GetAvailableContents("stone"));
        Assert.AreEqual(8.0f, w.GetAvailableStorage("stone"));
        Assert.AreEqual(2.0f, w.GetTotalContents("stone"));

        w.WithdrawReservation(res2);
        Assert.AreEqual(9.0f, w.GetAvailableStorage("stone"));
        Assert.AreEqual(1.0f, w.GetAvailableContents("stone"));
        Assert.AreEqual(1.0f, w.GetTotalContents("stone"));

        Assert.False(w.ReserveContents(go2, "stone", 3.0f));
        Assert.True(w.ReserveStorage(go2, "stone", 3.0f));
        var res3 = go2.GetComponent<StorageReservation>();
        Assert.IsNotNull(res3);
        Assert.AreEqual(6.0f, w.GetAvailableStorage("stone"));
        Assert.AreEqual(1.0f, w.GetAvailableContents("stone"));
        Assert.False(w.ReserveContents(go2, "stone", 2.0f));
        var res4 = go2.GetComponent<ResourceReservation>();
        Assert.IsNull(res4);

        w.DepositReservation(res3);
        Assert.AreEqual(6.0f, w.GetAvailableStorage("stone"));
        Assert.AreEqual(4.0f, w.GetAvailableContents("stone"));
        Assert.AreEqual(4.0f, w.GetTotalContents("stone"));
        Assert.False(w.ReserveContents(go2, "stone", 5.0f));
        Assert.IsNull(go2.GetComponent<ResourceReservation>());
    }
}
