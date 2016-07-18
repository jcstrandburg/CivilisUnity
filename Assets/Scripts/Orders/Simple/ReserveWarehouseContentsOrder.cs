using UnityEngine;
using System.Collections;

/// <summary>
/// Order to reserve the given resrouces from any available warehouse
/// </summary>
public class ReserveWarehouseContentsOrder : BaseOrder {
    string resourceType;
    float amount;

    public ReserveWarehouseContentsOrder(ActorController a, string resourceType, float amount) : base(a) {
        this.resourceType = resourceType;
        this.amount = amount;
    }

    public override void DoStep() {
        if (GameController.instance.ReserveWarehouseResources(actor, resourceType, amount)) {
            completed = true;
        }
    }
}
