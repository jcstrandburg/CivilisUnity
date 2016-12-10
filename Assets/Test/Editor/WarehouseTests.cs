using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[TestFixture]
[Category("Warehouse Tests")]
public class WarehouseTests : NeolithicTest {

    /// <summary>
    /// Tests that when you set resource contents with profiles missing for 
    /// items present in the resource limits, the holes in the contents get 
    /// filled in to allow those resources to be deposited
    /// (This test sets limits before setting contents)
    /// </summary>
    [Test]
    public void TestContentsMissingFromLimits1() {
        Warehouse w = MakePlainComponent<Warehouse>();

        var resourceLimits = new ResourceProfile[] {
            new ResourceProfile("wood", 2),
            new ResourceProfile("fish", 2),
            new ResourceProfile("gold", 2),
            new ResourceProfile("stone", 2),
        };
        var resourceContents = new ResourceProfile[] {
            new ResourceProfile("fish", 2),
            new ResourceProfile("gold", 2),
        };

        w.SetLimits(resourceLimits);
        w.SetContents(resourceContents);

        Assert.False(w.ReserveStorage(dummyObject, "stone", 3));
        Assert.IsNull(dummyObject.GetComponent<StorageReservation>());
        Assert.True(w.ReserveStorage(dummyObject, "stone", 1));
        var res = dummyObject.GetComponent<StorageReservation>();
        Assert.IsNotNull(res);

        Assert.True(res.Ready);
        w.DepositReservation(res);
        Assert.True(res.Released);
        Assert.AreEqual(1, w.GetAvailableContents("stone"));

        Assert.False(w.ReserveContents(dummyObject, "wood", 1));
        Assert.IsNull(dummyObject.GetComponent<ResourceReservation>());
        Assert.True(w.ReserveContents(dummyObject, "stone", 1));
        var res2 = dummyObject.GetComponent<ResourceReservation>();
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
        Warehouse w = MakePlainComponent<Warehouse>();

        var resourceLimits = new ResourceProfile[] {
            new ResourceProfile("wood", 2),
            new ResourceProfile("fish", 2),
            new ResourceProfile("gold", 2),
            new ResourceProfile("stone", 2),
        };
        var resourceContents = new ResourceProfile[] {
            new ResourceProfile("fish", 2),
            new ResourceProfile("gold", 2),
        };

        w.SetContents(resourceContents);
        w.SetLimits(resourceLimits);

        Assert.False(w.ReserveStorage(dummyObject, "stone", 3));
        Assert.IsNull(dummyObject.GetComponent<StorageReservation>());
        Assert.True(w.ReserveStorage(dummyObject, "stone", 1));
        var res = dummyObject.GetComponent<StorageReservation>();
        Assert.IsNotNull(res);

        Assert.True(res.Ready);
        w.DepositReservation(res);
        Assert.True(res.Released);
        Assert.AreEqual(1, w.GetAvailableContents("stone"));

        Assert.False(w.ReserveContents(dummyObject, "wood", 1));
        Assert.IsNull(dummyObject.GetComponent<ResourceReservation>());
        Assert.True(w.ReserveContents(dummyObject, "stone", 1));
        var res2 = dummyObject.GetComponent<ResourceReservation>();
        Assert.IsNotNull(res2);
        w.WithdrawReservation(res2);
        Assert.True(res2.Released);
    }

    [Test]
    public void TestContentsAndSpaceAvailability() {
        Warehouse w = MakePlainComponent<Warehouse>();
        w.SetLimits(new ResourceProfile[] {
            new ResourceProfile("stone", 10),
        });

        Assert.AreEqual(0, w.GetTotalContents("stone"));
        Assert.AreEqual(0, w.GetReservedStorage("stone"));
        Assert.AreEqual(10, w.GetAvailableStorage("stone"));

        Assert.True(w.ReserveStorage(dummyObject, "stone", 2));
        var res = dummyObject.GetComponent<StorageReservation>();
        Assert.IsNotNull(res);
        Assert.AreEqual(8, w.GetAvailableStorage("stone"));
        Assert.AreEqual(0, w.GetAvailableContents("stone"));
        w.DepositReservation(res);
        Assert.True(res.Released);
        Assert.AreEqual(2, w.GetAvailableContents("stone"));
        Assert.AreEqual(2, w.GetTotalContents("stone"));

        Assert.True(w.ReserveContents(dummyObject, "stone", 1));
        var res2 = dummyObject.GetComponent<ResourceReservation>();
        Assert.IsNotNull(res2);
        Assert.AreEqual(1, w.GetReservedContents("stone"));
        Assert.AreEqual(1, w.GetAvailableContents("stone"));
        Assert.AreEqual(8, w.GetAvailableStorage("stone"));
        Assert.AreEqual(2, w.GetTotalContents("stone"));

        w.WithdrawReservation(res2);
        Assert.AreEqual(9, w.GetAvailableStorage("stone"));
        Assert.AreEqual(1, w.GetAvailableContents("stone"));
        Assert.AreEqual(1, w.GetTotalContents("stone"));

        Assert.False(w.ReserveContents(dummyObject, "stone", 3));
        Assert.True(w.ReserveStorage(dummyObject, "stone", 3));
        var res3 = dummyObject.GetComponent<StorageReservation>();
        Assert.IsNotNull(res3);
        Assert.AreEqual(6, w.GetAvailableStorage("stone"));
        Assert.AreEqual(1, w.GetAvailableContents("stone"));
        Assert.False(w.ReserveContents(dummyObject, "stone", 2));
        var res4 = dummyObject.GetComponent<ResourceReservation>();
        Assert.IsNull(res4);

        w.DepositReservation(res3);
        Assert.AreEqual(6, w.GetAvailableStorage("stone"));
        Assert.AreEqual(4, w.GetAvailableContents("stone"));
        Assert.AreEqual(4, w.GetTotalContents("stone"));
        Assert.False(w.ReserveContents(dummyObject, "stone", 5));
        Assert.IsNull(dummyObject.GetComponent<ResourceReservation>());
    }

    [Test]
    public void TestGetAllAvailableContents() {
        var w = MakePlainComponent<Warehouse>();
        var resourceContents = new ResourceProfile[] {
            new ResourceProfile("fish", 2),
            new ResourceProfile("gold", 2),
        };

        w.SetContents(resourceContents);
        var availables = w.GetAllAvailableContents();
        CollectionAssert.AreEquivalent(
            new Dictionary<string, float>() {
                { "fish", 2 },
                { "gold", 2 },
            },
            availables);

        w.ReserveContents(dummyObject, "fish", 2);
        w.ReserveContents(dummyObject, "gold", 1);
        availables = w.GetAllAvailableContents();
        Assert.AreEqual(0, availables["fish"]);
        Assert.AreEqual(1, availables["gold"]);
    }
}
