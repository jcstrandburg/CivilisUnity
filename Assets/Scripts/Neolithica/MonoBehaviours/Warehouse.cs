using System;
using System.Collections.Generic;
using System.Linq;
using Neolithica.DependencyInjection;
using Neolithica.MonoBehaviours.Reservations;
using Tofu.Serialization;
using UnityEngine;

namespace Neolithica.MonoBehaviours {
    // TODO: Remove finished reservations
    // TODO: Check to make sure reservations are for this warehouse before fulfilling them
    [SavableMonobehaviour(2)]
    public class Warehouse : MonoBehaviour {
        public double totalCapacity;
        public ResourceProfile[] resourceLimits = new ResourceProfile[] { };
        public ResourceProfile[] resourceContents = new ResourceProfile[] { };
        public List<StorageReservation> storageReservations = new List<StorageReservation>();
        public List<ResourceReservation> resourceReservations = new List<ResourceReservation>();

        [Inject]
        public GameController GameController { get; set; }
        [Inject]
        private GameFactoryBase Factory { get; set; }

        /// <summary>
        /// Deposits the contents of the given reservation into this warehouse
        /// </summary>
        /// <param name="res">A reservation (for this warehosue)</param>
        public void DepositReservation(StorageReservation res) {
            if (!res.Ready) {
                throw new Exception("Reservation is not ready");
            }
            foreach (ResourceProfile rp in resourceContents) {
                if (rp.ResourceKind == res.resourceResourceKind) {
                    res.Released = true;
                    rp.Amount += res.amount;
                    storageReservations.Remove(res);
                    SendMessage("OnResourceDeposited", SendMessageOptions.DontRequireReceiver);
                    return;
                }
            }
            throw new Exception("Unable to deposit reservation");
        }


        /// <summary>
        /// Creates ResourceProfiles for any resource tags that are present in limits but not in 
        /// </summary>
        public void FillInContentsGaps() {
            //var diff = resourceLimits.Select(rp => rp.type).Except(resourceContents.Select(rp => rp.type));
            //var toAdd = diff.Select(resourceType => new ResourceProfile(resourceType, 0)).ToList();
            var toAdd = resourceLimits
                .Select(rp => rp.ResourceKind)
                .Except(resourceContents.Select(rp => rp.ResourceKind))
                .Select(type => new ResourceProfile(type, 0));
            resourceContents = resourceContents.Union(toAdd).ToArray();


        }

        /// <summary>
        /// Sets resource limits based on the given collection of ResourceProfiles
        /// </summary>
        /// <param name="limits"></param>
        public void SetLimits(IEnumerable<ResourceProfile> limits) {
            List<ResourceProfile> list = new List<ResourceProfile>();
            foreach (ResourceProfile r in limits) {
                list.Add((ResourceProfile)r.Clone());
            }
            resourceLimits = list.ToArray();
            FillInContentsGaps();
        }

        /// <summary>
        /// Sets resource contents based on the given collection of ResourceProfiles
        /// </summary>
        /// <param name="limits"></param>
        public void SetContents(IEnumerable<ResourceProfile> contents) {
            List<ResourceProfile> list = new List<ResourceProfile>();
            foreach (ResourceProfile r in contents) {
                list.Add((ResourceProfile)r.Clone());
            }
            resourceContents = list.ToArray();
            FillInContentsGaps();
        }

        public double GetTotalAnyContents() {
            if (!enabled) { return 0; }
            double t = 0;
            foreach (ResourceProfile rp in resourceContents) {
                t += rp.Amount;
            }
            return t;
        }

        /// <summary>
        /// Gets the amount of the given resource, whether reserved or not
        /// </summary>
        /// <param name="resourceKind"></param>
        /// <returns>Total deposited resources for the given tag</returns>
        public double GetTotalContents(ResourceKind resourceKind) {
            if (!enabled) { return 0; }
            foreach (ResourceProfile rp in resourceContents) {
                if (rp.ResourceKind == resourceKind) {
                    return rp.Amount;
                }
            }
            return 0;
        }

        /// <summary>
        /// Get the total reserved content
        /// </summary>
        /// <param name="resourceKind"></param>
        /// <returns></returns>
        public double GetReservedContents(ResourceKind resourceKind) {
            double amount = 0 ;

            foreach (ResourceReservation r in resourceReservations) {
                if (r.resourceKind == resourceKind) {
                    amount += r.amount;
                }
            }
            return amount;
        }

        /// <summary>
        /// Gets total available contents for this warehouse
        /// </summary>
        /// <returns>Available contents in a dictionary</returns>
        public Dictionary<ResourceKind, double> GetAllAvailableContents() {
            var d = new Dictionary<ResourceKind, double>();
            if (!enabled) { return d; }
            foreach (var r in resourceContents) {
                d[r.ResourceKind] = GetAvailableContents(r.ResourceKind);
            }
            return d;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resourceKind"></param>
        /// <returns></returns>
        public double GetAvailableContents(ResourceKind resourceKind) {
            if (!enabled) { return 0; }
            return GetTotalContents(resourceKind) - GetReservedContents(resourceKind);
        }

        /// <summary>
        /// Gets the total number of resources (with the given tag) with reservations on them
        /// </summary>
        /// <param name="resourceTag"></param>
        /// <returns></returns>
        public double GetClaimedContents(ResourceKind resourceKind) {
            if (!enabled) { return 0; }
            double amount = 0;
            foreach (ResourceReservation r in resourceReservations) {
                if (r.resourceKind == resourceKind && r.Ready) {
                    amount += r.amount;
                }
            }
            return amount;
        }

        /// <summary>
        /// Gets total contents without reservations on them (for the given tag)
        /// </summary>
        /// <param name="resourceTag"></param>
        /// <returns></returns>
        public double GetUnclaimedContents(ResourceKind resourceKind) {
            if (!enabled) { return 0; }
            return GetTotalContents(resourceKind) - GetClaimedContents(resourceKind);
        }

        /// <summary>
        /// Gets the total amount of space allocated for the given type of resource (doesn't not deduct space currently used!
        /// </summary>
        /// <param name="resourceTag"></param>
        /// <returns></returns>
        public double GetTotalStorage(ResourceKind resourceKind) {
            if (!enabled) { return 0; }
            double amount = 0;
            foreach (ResourceProfile p in resourceLimits) {
                if (p.ResourceKind == resourceKind) {
                    amount += p.Amount;
                }
            }
            return amount;
        }

        /// <summary>
        /// Gets the total amount of space currently reserved (but not occupied) for the given resource tag
        /// </summary>
        /// <param name="resourceKind"></param>
        /// <returns></returns>
        public double GetReservedStorage(ResourceKind resourceKind) {
            if (!enabled) { return 0; }
            double amount = 0;
            foreach (StorageReservation r in storageReservations) {
                if (!r.Released) {
                    amount += r.amount;
                }
            }
            return amount;
        }

        /// <summary>
        /// Gets the total unreserved space currently available for the given resource tag
        /// </summary>
        /// <param name="resourceTag"></param>
        /// <returns></returns>
        public double GetAvailableStorage(ResourceKind resourceKind) {
            if (!enabled) { return 0; }
            return GetTotalStorage(resourceKind) - GetTotalContents(resourceKind) - GetReservedStorage(resourceKind);
        }

        /// <summary>
        /// Directly withdraws the specified resource without a reservation
        /// </summary>
        /// <param name="resourceKind"></param>
        /// <param name="amount"></param>
        public void WithdrawContents(ResourceKind resourceKind, double amount) {
            foreach (ResourceProfile rp in resourceContents) {
                if (rp.ResourceKind == resourceKind && rp.Amount >= amount) {
                    rp.Amount -= amount;
                    SendMessage("OnResourceWithdrawn", SendMessageOptions.DontRequireReceiver);
                    return;
                }
            }
            throw new InvalidOperationException("Unable to withdraw reservation");
        }

        /// <summary>
        /// Removes the resources for the given reservation from this warehouse and sets the reservation to released
        /// </summary>
        /// <param name="res"></param>
        public void WithdrawReservation(ResourceReservation res) {
            if (!res.Ready) {
                throw new Exception("Reservation is not ready");
            }
            if (res.source != gameObject) {
                throw new Exception ("Attempting to withdraw reservation for another warehouse!");
            }
            if (res.Released) {
                throw new Exception ("Reservation already filled!");
            }

            foreach (ResourceProfile rp in resourceContents) {
                if (rp.ResourceKind == res.resourceKind && rp.Amount >= res.amount) {
                    rp.Amount -= res.amount;
                    res.Released = true;
                    resourceReservations.Remove(res);
                    SendMessage("OnResourceWithdrawn", SendMessageOptions.DontRequireReceiver);
                    return;
                }
            }
            //Debug.Log(res);
            throw new InvalidOperationException("Unable to withdraw reservation");
        }

        /// <summary>
        /// Attempts to reserve the resource contents of this warehouse, returning a boolean indicating whether the operation was successful
        /// </summary>
        /// <param name="reserver">Gameobject to which this reservation should be attached</param>
        /// <param name="resourceKind">Resource type tag</param>
        /// <param name="amount">Amount to reserve</param>
        /// <returns>true on success, false on failure</returns>
        public bool ReserveContents(GameObject reserver, ResourceKind resourceKind, double amount) {
            double avail = GetAvailableContents(resourceKind);
            if (avail < amount) {
                return false;
            }
            ResourceReservation r = reserver.AddComponent<ResourceReservation>();
            r.amount = amount;
            r.resourceKind = resourceKind;
            r.Ready = true;
            r.source = gameObject;
            resourceReservations.Add(r);
            return true;
        }

        /// <summary>
        /// Attempts to reserve resource storage in this warehouse, returning a boolean indicating whether the operation was successful
        /// </summary>
        /// <param name="reserver">GameObject to which this reservation should be attached</param>
        /// <param name="resourceKind">Resource type tag</param>
        /// <param name="amount">Amount to reserve</param>
        /// <returns>true on success, false on failure</returns>
        public bool ReserveStorage(GameObject reserver, ResourceKind resourceKind, double amount) {
            if (GetAvailableStorage(resourceKind) < amount)
                return false;

            StorageReservation r = Factory.AddComponent<StorageReservation>(reserver);
            r.warehouse = this;
            r.amount = amount;
            r.resourceResourceKind = resourceKind;
            r.Ready = true;
            storageReservations.Add(r);
            return true;
        }

        public void FixedUpdate() {
            storageReservations.RemoveAll((r) => { return r.Released || r.Cancelled; });
            resourceReservations.RemoveAll((r) => { return r.Released || r.Cancelled; });
        }

        public void OnDestroy() {
            storageReservations.ForEach((r) => { if (!r.Released) r.Cancelled = true; });
            resourceReservations.ForEach((r) => { if (!r.Released) r.Cancelled = true; });
        }

        public void OnTearDown() {
            Debug.Log("Tearing down");
            foreach (var rc in resourceContents) {
                while (rc.Amount > 0) {
                    rc.Amount -= 1;

                    var resource = GameController.CreateResourcePile(rc.ResourceKind, Math.Min(rc.Amount, 1.0));
                    resource.transform.position = transform.position;
                    resource.GetComponent<Resource>().SetDown();
                }
            }
        }
    }
}
