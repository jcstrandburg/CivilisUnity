using UnityEngine;
using System.Collections;

/// <summary>
/// Order to store the resources for the given StorageReservation in any available warehouse
/// </summary>
public class StoreReservationOrder : BaseOrder {
    StorageReservation res;

    public StoreReservationOrder(ActorController a, StorageReservation r) : base(a) {
        res = r;
    }

    public override void DoStep() {
        if (actor.MoveTowards(res.warehouse.transform.position)) {
            res.warehouse.DepositReservation(res);
            GameObject.Destroy(actor.GetCarriedResource(res.resourceType).gameObject);
            res.Released = true;
            this.completed = true;
        }
    }
}
