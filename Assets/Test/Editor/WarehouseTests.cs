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
            new ResourceProfile(Resource.Type.Wood, 2),
            new ResourceProfile(Resource.Type.Fish, 2),
            new ResourceProfile(Resource.Type.Gold, 2),
            new ResourceProfile(Resource.Type.Stone, 2),
        };
        var resourceContents = new ResourceProfile[] {
            new ResourceProfile(Resource.Type.Fish, 2),
            new ResourceProfile(Resource.Type.Gold, 2),
        };

        w.SetLimits(resourceLimits);
        w.SetContents(resourceContents);

        Assert.False(w.ReserveStorage(dummyObject, Resource.Type.Stone, 3));
        Assert.IsNull(dummyObject.GetComponent<StorageReservation>());
        Assert.True(w.ReserveStorage(dummyObject, Resource.Type.Stone, 1));
        var res = dummyObject.GetComponent<StorageReservation>();
        Assert.IsNotNull(res);

        Assert.True(res.Ready);
        w.DepositReservation(res);
        Assert.True(res.Released);
        Assert.AreEqual(1, w.GetAvailableContents(Resource.Type.Stone));

        Assert.False(w.ReserveContents(dummyObject, Resource.Type.Wood, 1));
        Assert.IsNull(dummyObject.GetComponent<ResourceReservation>());
        Assert.True(w.ReserveContents(dummyObject, Resource.Type.Stone, 1));
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
            new ResourceProfile(Resource.Type.Wood, 2),
            new ResourceProfile(Resource.Type.Fish, 2),
            new ResourceProfile(Resource.Type.Gold, 2),
            new ResourceProfile(Resource.Type.Stone, 2),
        };
        var resourceContents = new ResourceProfile[] {
            new ResourceProfile(Resource.Type.Fish, 2),
            new ResourceProfile(Resource.Type.Gold, 2),
        };

        w.SetContents(resourceContents);
        w.SetLimits(resourceLimits);

        Assert.False(w.ReserveStorage(dummyObject, Resource.Type.Stone, 3));
        Assert.IsNull(dummyObject.GetComponent<StorageReservation>());
        Assert.True(w.ReserveStorage(dummyObject, Resource.Type.Stone, 1));
        var res = dummyObject.GetComponent<StorageReservation>();
        Assert.IsNotNull(res);

        Assert.True(res.Ready);
        w.DepositReservation(res);
        Assert.True(res.Released);
        Assert.AreEqual(1, w.GetAvailableContents(Resource.Type.Stone));

        Assert.False(w.ReserveContents(dummyObject, Resource.Type.Wood, 1));
        Assert.IsNull(dummyObject.GetComponent<ResourceReservation>());
        Assert.True(w.ReserveContents(dummyObject, Resource.Type.Stone, 1));
        var res2 = dummyObject.GetComponent<ResourceReservation>();
        Assert.IsNotNull(res2);
        w.WithdrawReservation(res2);
        Assert.True(res2.Released);
    }

    [Test]
    public void TestContentsAndSpaceAvailability() {
        Warehouse w = MakePlainComponent<Warehouse>();
        w.SetLimits(new ResourceProfile[] {
            new ResourceProfile(Resource.Type.Stone, 10),
        });

        Assert.AreEqual(0, w.GetTotalContents(Resource.Type.Stone));
        Assert.AreEqual(0, w.GetReservedStorage(Resource.Type.Stone));
        Assert.AreEqual(10, w.GetAvailableStorage(Resource.Type.Stone));

        Assert.True(w.ReserveStorage(dummyObject, Resource.Type.Stone, 2));
        var res = dummyObject.GetComponent<StorageReservation>();
        Assert.IsNotNull(res);
        Assert.AreEqual(8, w.GetAvailableStorage(Resource.Type.Stone));
        Assert.AreEqual(0, w.GetAvailableContents(Resource.Type.Stone));
        w.DepositReservation(res);
        Assert.True(res.Released);
        Assert.AreEqual(2, w.GetAvailableContents(Resource.Type.Stone));
        Assert.AreEqual(2, w.GetTotalContents(Resource.Type.Stone));

        Assert.True(w.ReserveContents(dummyObject, Resource.Type.Stone, 1));
        var res2 = dummyObject.GetComponent<ResourceReservation>();
        Assert.IsNotNull(res2);
        Assert.AreEqual(1, w.GetReservedContents(Resource.Type.Stone));
        Assert.AreEqual(1, w.GetAvailableContents(Resource.Type.Stone));
        Assert.AreEqual(8, w.GetAvailableStorage(Resource.Type.Stone));
        Assert.AreEqual(2, w.GetTotalContents(Resource.Type.Stone));

        w.WithdrawReservation(res2);
        Assert.AreEqual(9, w.GetAvailableStorage(Resource.Type.Stone));
        Assert.AreEqual(1, w.GetAvailableContents(Resource.Type.Stone));
        Assert.AreEqual(1, w.GetTotalContents(Resource.Type.Stone));

        Assert.False(w.ReserveContents(dummyObject, Resource.Type.Stone, 3));
        Assert.True(w.ReserveStorage(dummyObject, Resource.Type.Stone, 3));
        var res3 = dummyObject.GetComponent<StorageReservation>();
        Assert.IsNotNull(res3);
        Assert.AreEqual(6, w.GetAvailableStorage(Resource.Type.Stone));
        Assert.AreEqual(1, w.GetAvailableContents(Resource.Type.Stone));
        Assert.False(w.ReserveContents(dummyObject, Resource.Type.Stone, 2));
        var res4 = dummyObject.GetComponent<ResourceReservation>();
        Assert.IsNull(res4);

        w.DepositReservation(res3);
        Assert.AreEqual(6, w.GetAvailableStorage(Resource.Type.Stone));
        Assert.AreEqual(4, w.GetAvailableContents(Resource.Type.Stone));
        Assert.AreEqual(4, w.GetTotalContents(Resource.Type.Stone));
        Assert.False(w.ReserveContents(dummyObject, Resource.Type.Stone, 5));
        Assert.IsNull(dummyObject.GetComponent<ResourceReservation>());
    }

    [Test]
    public void TestGetAllAvailableContents() {
        var w = MakePlainComponent<Warehouse>();
        var resourceContents = new ResourceProfile[] {
            new ResourceProfile(Resource.Type.Fish, 2),
            new ResourceProfile(Resource.Type.Gold, 2),
        };

        w.SetContents(resourceContents);
        var availables = w.GetAllAvailableContents();
        CollectionAssert.AreEquivalent(
            new Dictionary<Resource.Type, float>() {
                { Resource.Type.Fish, 2 },
                { Resource.Type.Gold, 2 },
            },
            availables);

        w.ReserveContents(dummyObject, Resource.Type.Fish, 2);
        w.ReserveContents(dummyObject, Resource.Type.Gold, 1);
        availables = w.GetAllAvailableContents();
        Assert.AreEqual(0, availables[Resource.Type.Fish]);
        Assert.AreEqual(1, availables[Resource.Type.Gold]);
    }
}
