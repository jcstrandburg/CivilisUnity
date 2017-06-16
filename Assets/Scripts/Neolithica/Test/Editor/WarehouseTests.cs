using System.Collections.Generic;
using Neolithica.MonoBehaviours;
using Neolithica.MonoBehaviours.Reservations;
using NUnit.Framework;

namespace Neolithica.Test.Editor {
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
            Warehouse w = MakeTestComponent<Warehouse>();

            var resourceLimits = new ResourceProfile[] {
                new ResourceProfile(ResourceKind.Wood, 2),
                new ResourceProfile(ResourceKind.Fish, 2),
                new ResourceProfile(ResourceKind.Gold, 2),
                new ResourceProfile(ResourceKind.Stone, 2),
            };
            var resourceContents = new ResourceProfile[] {
                new ResourceProfile(ResourceKind.Fish, 2),
                new ResourceProfile(ResourceKind.Gold, 2),
            };

            w.SetLimits(resourceLimits);
            w.SetContents(resourceContents);

            Assert.False(w.ReserveStorage(dummyObject, ResourceKind.Stone, 3));
            Assert.IsNull(dummyObject.GetComponent<StorageReservation>());
            Assert.True(w.ReserveStorage(dummyObject, ResourceKind.Stone, 1));
            var res = dummyObject.GetComponent<StorageReservation>();
            Assert.IsNotNull(res);

            Assert.True(res.Ready);
            w.DepositReservation(res);
            Assert.True(res.Released);
            Assert.AreEqual(1, w.GetAvailableContents(ResourceKind.Stone));

            Assert.False(w.ReserveContents(dummyObject, ResourceKind.Wood, 1));
            Assert.IsNull(dummyObject.GetComponent<ResourceReservation>());
            Assert.True(w.ReserveContents(dummyObject, ResourceKind.Stone, 1));
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
            Warehouse w = MakeTestComponent<Warehouse>();

            var resourceLimits = new ResourceProfile[] {
                new ResourceProfile(ResourceKind.Wood, 2),
                new ResourceProfile(ResourceKind.Fish, 2),
                new ResourceProfile(ResourceKind.Gold, 2),
                new ResourceProfile(ResourceKind.Stone, 2),
            };
            var resourceContents = new ResourceProfile[] {
                new ResourceProfile(ResourceKind.Fish, 2),
                new ResourceProfile(ResourceKind.Gold, 2),
            };

            w.SetContents(resourceContents);
            w.SetLimits(resourceLimits);

            Assert.False(w.ReserveStorage(dummyObject, ResourceKind.Stone, 3));
            Assert.IsNull(dummyObject.GetComponent<StorageReservation>());
            Assert.True(w.ReserveStorage(dummyObject, ResourceKind.Stone, 1));
            var res = dummyObject.GetComponent<StorageReservation>();
            Assert.IsNotNull(res);

            Assert.True(res.Ready);
            w.DepositReservation(res);
            Assert.True(res.Released);
            Assert.AreEqual(1, w.GetAvailableContents(ResourceKind.Stone));

            Assert.False(w.ReserveContents(dummyObject, ResourceKind.Wood, 1));
            Assert.IsNull(dummyObject.GetComponent<ResourceReservation>());
            Assert.True(w.ReserveContents(dummyObject, ResourceKind.Stone, 1));
            var res2 = dummyObject.GetComponent<ResourceReservation>();
            Assert.IsNotNull(res2);
            w.WithdrawReservation(res2);
            Assert.True(res2.Released);
        }

        [Test]
        public void TestContentsAndSpaceAvailability() {
            Warehouse w = MakeTestComponent<Warehouse>();
            w.SetLimits(new ResourceProfile[] {
                new ResourceProfile(ResourceKind.Stone, 10),
            });

            Assert.AreEqual(0, w.GetTotalContents(ResourceKind.Stone));
            Assert.AreEqual(0, w.GetReservedStorage(ResourceKind.Stone));
            Assert.AreEqual(10, w.GetAvailableStorage(ResourceKind.Stone));

            Assert.True(w.ReserveStorage(dummyObject, ResourceKind.Stone, 2));
            var res = dummyObject.GetComponent<StorageReservation>();
            Assert.IsNotNull(res);
            Assert.AreEqual(8, w.GetAvailableStorage(ResourceKind.Stone));
            Assert.AreEqual(0, w.GetAvailableContents(ResourceKind.Stone));
            w.DepositReservation(res);
            Assert.True(res.Released);
            Assert.AreEqual(2, w.GetAvailableContents(ResourceKind.Stone));
            Assert.AreEqual(2, w.GetTotalContents(ResourceKind.Stone));

            Assert.True(w.ReserveContents(dummyObject, ResourceKind.Stone, 1));
            var res2 = dummyObject.GetComponent<ResourceReservation>();
            Assert.IsNotNull(res2);
            Assert.AreEqual(1, w.GetReservedContents(ResourceKind.Stone));
            Assert.AreEqual(1, w.GetAvailableContents(ResourceKind.Stone));
            Assert.AreEqual(8, w.GetAvailableStorage(ResourceKind.Stone));
            Assert.AreEqual(2, w.GetTotalContents(ResourceKind.Stone));

            w.WithdrawReservation(res2);
            Assert.AreEqual(9, w.GetAvailableStorage(ResourceKind.Stone));
            Assert.AreEqual(1, w.GetAvailableContents(ResourceKind.Stone));
            Assert.AreEqual(1, w.GetTotalContents(ResourceKind.Stone));

            Assert.False(w.ReserveContents(dummyObject, ResourceKind.Stone, 3));
            Assert.True(w.ReserveStorage(dummyObject, ResourceKind.Stone, 3));
            var res3 = dummyObject.GetComponent<StorageReservation>();
            Assert.IsNotNull(res3);
            Assert.AreEqual(6, w.GetAvailableStorage(ResourceKind.Stone));
            Assert.AreEqual(1, w.GetAvailableContents(ResourceKind.Stone));
            Assert.False(w.ReserveContents(dummyObject, ResourceKind.Stone, 2));
            var res4 = dummyObject.GetComponent<ResourceReservation>();
            Assert.IsNull(res4);

            w.DepositReservation(res3);
            Assert.AreEqual(6, w.GetAvailableStorage(ResourceKind.Stone));
            Assert.AreEqual(4, w.GetAvailableContents(ResourceKind.Stone));
            Assert.AreEqual(4, w.GetTotalContents(ResourceKind.Stone));
            Assert.False(w.ReserveContents(dummyObject, ResourceKind.Stone, 5));
            Assert.IsNull(dummyObject.GetComponent<ResourceReservation>());
        }

        [Test]
        public void TestGetAllAvailableContents() {
            var w = MakePlainComponent<Warehouse>();
            var resourceContents = new ResourceProfile[] {
                new ResourceProfile(ResourceKind.Fish, 2),
                new ResourceProfile(ResourceKind.Gold, 2),
            };

            w.SetContents(resourceContents);
            var availables = w.GetAllAvailableContents();
            CollectionAssert.AreEquivalent(
                new Dictionary<ResourceKind, float>() {
                    { ResourceKind.Fish, 2 },
                    { ResourceKind.Gold, 2 },
                },
                availables);

            w.ReserveContents(dummyObject, ResourceKind.Fish, 2);
            w.ReserveContents(dummyObject, ResourceKind.Gold, 1);
            availables = w.GetAllAvailableContents();
            Assert.AreEqual(0, availables[ResourceKind.Fish]);
            Assert.AreEqual(1, availables[ResourceKind.Gold]);
        }
    }
}
