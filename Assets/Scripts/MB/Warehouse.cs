using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof (Warehouse))]
class WarehouseEditor : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        Warehouse w = (Warehouse)target;
        if (GUILayout.Button("Normalize Contents/Limits")) {
            w.FillInContentsGaps();
        }
    }
}
#endif

// TODO: Remove finished reservations
// TODO: Check to make sure reservations are for this warehouse before fulfilling them
public class Warehouse : MonoBehaviour {
    public float totalCapacity;
    [SerializeField]
    public ResourceProfile[] resourceLimits = new ResourceProfile[] { };
    [SerializeField]
    public ResourceProfile[] resourceContents = new ResourceProfile[] { };
    [SerializeField]
    public List<StorageReservation> storageReservations = new List<StorageReservation>();
    [SerializeField]
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
        var diff = (from rp in resourceLimits select rp.resourceTag)
                    .Except(from rp in resourceContents select rp.resourceTag);
        var toAdd = new List<ResourceProfile>();
        foreach (String resourceTag in diff) {
            toAdd.Add(new ResourceProfile(resourceTag, 0));
        }
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

    public float GetTotalAnyContents() {
        if (!enabled) { return 0; }
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
        if (!enabled) { return 0; }
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
        //Debug.Log("GetReservedContents: " + resourceReservations.Count);
        foreach (ResourceReservation r in resourceReservations) {
            //Debug.Log(r.resourceTag + " " + r.amount);
            if (r.resourceTag == resourceTag) {
                //Debug.Log(r.amount);
                amount += r.amount;
            }
        }
        //Debug.Log("Total: " + amount);
        return amount;
    }

    /// <summary>
    /// Gets total available contents for this warehouse
    /// </summary>
    /// <returns>Available contents in a dictionary</returns>
    public Dictionary<string, float> GetAllAvailableContents() {
        var d = new Dictionary<string, float>();
        if (!enabled) { return d; }
        foreach (var r in resourceContents) {
            d[r.resourceTag] = GetAvailableContents(r.resourceTag);
        }
        return d;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="resourceTag"></param>
    /// <returns></returns>
    public float GetAvailableContents(string resourceTag) {
        if (!enabled) { return 0; }
        return GetTotalContents(resourceTag) - GetReservedContents(resourceTag);
    }

    /// <summary>
	/// Gets the total number of resources (with the given tag) with reservations on them
    /// </summary>
    /// <param name="resourceTag"></param>
    /// <returns></returns>
    public float GetClaimedContents(string resourceTag) {
        if (!enabled) { return 0; }
        float amount = 0.0f;
        foreach (ResourceReservation r in resourceReservations) {
            if (r.resourceTag == resourceTag && r.Ready) {
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
        if (!enabled) { return 0; }
        return GetTotalContents(resourceTag) - GetClaimedContents(resourceTag);
    }

    /// <summary>
    /// Gets the total amount of space allocated for the given type of resource (doesn't not deduct space currently used!
    /// </summary>
    /// <param name="resourceTag"></param>
    /// <returns></returns>
    public float GetTotalStorage(string resourceTag) {
        if (!enabled) { return 0; }
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
        if (!enabled) { return 0; }
        float amount = 0.0f;
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
    public float GetAvailableStorage(string resourceTag) {
        if (!enabled) { return 0; }
        return GetTotalStorage(resourceTag) - GetTotalContents(resourceTag) - GetReservedStorage(resourceTag);
    }

    /// <summary>
    /// Directly withdraws the specified resource without a reservation
    /// </summary>
    /// <param name="resTag"></param>
    /// <param name="amount"></param>
    public void WithdrawContents(string resTag, float amount) {
        foreach (ResourceProfile rp in resourceContents) {
            if (rp.resourceTag == resTag && rp.amount >= amount) {
                rp.amount -= amount;
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
        //Debug.Log(r.resourceTag + " " + r.amount);
        //Debug.Log("Res count: " + resourceReservations.Count);
        resourceReservations.Add(r);
        //Debug.Log("Res count: "+resourceReservations.Count);
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

    public void OnTearDown() {
        Debug.Log("Tearing down");
        foreach (var rc in resourceContents) {
            while (rc.amount > 0.0f) {
                rc.amount -= 1.0f;
                var resource = GameController.instance.CreateResourcePile(rc.resourceTag, Mathf.Min(rc.amount, 1.0f));
                resource.transform.position = transform.position;
                resource.GetComponent<Resource>().SetDown();
            }
        }
    }
}
