using UnityEngine;
using System.Collections;

/// <summary>
/// Order to reserve storage for the current carried resource in any available warehouse
/// </summary>
public class ReserveStorageOrder : BaseOrder {
    Resource resource;

    public ReserveStorageOrder(ActorController a) : base(a) {
        resource = a.GetCarriedResource();
        if (!resource) {
            failed = true;
        }
    }

    public override void DoStep() {
        string type = resource.typeTag;
        double amount = resource.amount;

        if (actor.gameController.ReserveStorage(actor, type, amount)) {
            Debug.Log("Reserved storage");
            completed = true;
        }
        else {
            failed = true;
        }
    }
}