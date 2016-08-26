using UnityEngine;
using System.Collections;

/// <summary>
/// Order to reserve the given resrouces from any available warehouse
/// </summary>
public class ReserveWarehouseContentsOrder : BaseOrder {
    string resourceType;
    double amount;

    public ReserveWarehouseContentsOrder(ActorController a, string resourceType, double amount) : base(a) {
        this.resourceType = resourceType;
        this.amount = amount;
    }

    public override void DoStep() {
        if (actor.gameController.ReserveWarehouseResources(actor, resourceType, amount)) {
            completed = true;
        }
    }
}
