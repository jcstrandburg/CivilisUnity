using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

[System.Serializable]
public class ResourceProfile : ICloneable {

    public ResourceProfile(string rt, float a) {
        resourceTag = rt;
        amount = a;
    }

    public object Clone() {
        return MemberwiseClone();
    }

    public string resourceTag;
    public float amount;
}

// TODO: Remove finished reservations
// TODO: Check to make sure reservations are for this warehouse before fulfilling them

public class Warehouse : MonoBehaviour {
    public float totalCapacity;
    public ResourceProfile[] resourceLimits = new ResourceProfile[] { };
    public ResourceProfile[] resourceContents = new ResourceProfile[] { };
    public List<StorageReservation> storageReservations = new List<StorageReservation>();
    public List<ResourceReservation> resourceReservations = new List<ResourceReservation>();

    /// <summary>
    /// Deposits the contents of the given reservation into this warehouse
    /// </summary>
    /// <param name="res">A reservation (for this warehosue)</param>
    public void DepositReservation(StorageReservation res) {
        if (!res.Ready) {
            throw new Exception("Reservation is not ready");
        }
        foreach (ResourceProfile rp in resourceContents) {
            if (rp.resourceTag == res.resourceTag) {
                res.Released = true;
                rp.amount += res.amount;
                SendMessage("OnResourceDeposited", SendMessageOptions.DontRequireReceiver);
                return;
            }
        }
        throw new Exception("Unable to deposit reservation");
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
    }

    public float GetTotalAnyContents() {
        float t = 0.0f;
        foreach (ResourceProfile rp in resourceContents) {
            t += rp.amount;
        }
        return t;
    }

    /// <summary>
    /// Gets the amount of the given resource, whether reserved or not
    /// </summary>
    /// <param name="resourceTag"></param>
    /// <returns>Total deposited resources for the given tag</returns>
    public float GetTotalContents(string resourceTag) {
        foreach (ResourceProfile rp in resourceContents) {
            if (rp.resourceTag == resourceTag) {
                return rp.amount;
            }
        }
        return 0.0f;
    }

    /// <summary>
    /// Get the total reserved content
    /// </summary>
    /// <param name="resourceTag"></param>
    /// <returns></returns>
    public float GetReservedContents(string resourceTag) {
        float amount = 0.0f;
        foreach (ResourceReservation r in resourceReservations) {
            if (r.tag == resourceTag) {
                amount += r.amount;
            }
        }
        return amount;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="resourceTag"></param>
    /// <returns></returns>
    public float GetAvailableContents(string resourceTag) {
        return GetTotalContents(resourceTag) - GetReservedContents(resourceTag);
    }

    /// <summary>
	/// Gets the total number of resources (with the given tag) with reservations on them
    /// </summary>
    /// <param name="resourceTag"></param>
    /// <returns></returns>
    public float GetClaimedContents(string resourceTag) {
        float amount = 0.0f;
        foreach (ResourceReservation r in resourceReservations) {
            if (r.tag == resourceTag && r.Ready) {
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
    public float GetUnclaimedContents(string resourceTag) {
        return GetTotalContents(resourceTag) - GetClaimedContents(resourceTag);
    }

    /// <summary>
    /// Gets the total amount of space allocated for the given type of resource (doesn't not deduct space currently used!
    /// </summary>
    /// <param name="resourceTag"></param>
    /// <returns></returns>
    public float GetTotalStorage(string resourceTag) {
        float amount = 0.0f;
        foreach (ResourceProfile p in resourceLimits) {
            if (p.resourceTag == resourceTag) {
                amount += p.amount;
            }
        }
        return amount;
    }

    /// <summary>
	/// Gets the total amount of space currently reserved (but not occupied) for the given resource tag
    /// </summary>
    /// <param name="resourceTag"></param>
    /// <returns></returns>
    public float GetReservedStorage(string resourceTag) {
        float amount = 0.0f;
        foreach (StorageReservation r in storageReservations) {
            amount += r.amount;
        }
        return amount;
    }

    /// <summary>
    /// Gets the total unreserved space currently available for the given resource tag
    /// </summary>
    /// <param name="resourceTag"></param>
    /// <returns></returns>
    public float GetAvailableStorage(string resourceTag) {
        return GetTotalStorage(resourceTag) - GetTotalContents(resourceTag) - GetReservedStorage(resourceTag);
    }

    /// <summary>
    /// Removes the resources for the given reservation from this warehouse and sets the reservation to released
    /// </summary>
    /// <param name="res"></param>
    public void WithdrawReservation(ResourceReservation res) {
        if (!res.Ready) {
            throw new Exception("Reservation is not ready");
        }
		if (res.source != this.gameObject) {
			throw new Exception ("Attempting to withdraw reservation for another warehouse!");
		}
		if (res.Released) {
			throw new Exception ("Reservation already filled!");
		}

        foreach (ResourceProfile rp in resourceContents) {
            if (rp.resourceTag == res.resourceTag && rp.amount >= res.amount) {
                rp.amount -= res.amount;
                res.Released = true;
                SendMessage("OnResourceWithdrawn", SendMessageOptions.DontRequireReceiver);
                return;
            }
        }
        Debug.Log(res);
        throw new Exception("Unable to withdraw reservation");
    }

    /// <summary>
    /// Attempts to reserve the resource contents of this warehouse, returning a boolean indicating whether the operation was successful
    /// </summary>
    /// <param name="reserver">Gameobject to which this reservation should be attached</param>
    /// <param name="resourceTag">Resource type tag</param>
    /// <param name="amount">Amount to reserve</param>
    /// <returns>true on success, false on failure</returns>
    public bool ReserveContents(GameObject reserver, string resourceTag, float amount) {
        float avail = GetAvailableContents(resourceTag);
        if (avail < amount) {
            return false;
        }
        ResourceReservation r = reserver.AddComponent<ResourceReservation>();
        r.amount = amount;
        r.resourceTag = resourceTag;
        r.Ready = true;
        r.source = this.gameObject;
        resourceReservations.Add(r);
        return true;
    }

    /// <summary>
    /// Attempts to reserve resource storage in this warehouse, returning a boolean indicating whether the operation was successful
    /// </summary>
    /// <param name="reserver">GameObject to which this reservation should be attached</param>
    /// <param name="resourceTag">Resource type tag</param>
    /// <param name="amount">Amount to reserve</param>
    /// <returns>true on success, false on failure</returns>
    public bool ReserveStorage(GameObject reserver, string resourceTag, float amount) {
        if (GetAvailableStorage(resourceTag) < amount) {
            return false;
        }
        StorageReservation r = reserver.AddComponent<StorageReservation>();
        r.warehouse = this;
        r.amount = amount;
        r.resourceTag = resourceTag;
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
}
