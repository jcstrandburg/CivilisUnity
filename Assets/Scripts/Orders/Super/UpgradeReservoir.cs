using UnityEngine;
using System.Collections;

/// <summary>
/// Order to upgrade a reservoir to it's next stage
/// </summary>
public class UpgradeReservoirOrder : StatefulSuperOrder {
    NeolithicObject targetObj;
    Reservoir reservoir;
    ResourceReservation resourceReservation;

    public UpgradeReservoirOrder(ActorController a, NeolithicObject target) : base(a) {
        targetObj = target;
        reservoir = target.GetComponent<Reservoir>();
        GoToState("seekTarget");
    }

    protected override void CreateStates() {
        CreateState("seekTarget",
            () => new SimpleMoveOrder(actor, targetObj.transform.position),
            () => GoToState("reservationWait"),
            null);
        CreateState("reservationWait",
            () => {
                resourceReservation = reservoir.NewReservation(actor.gameObject, 1);
                return new WaitForReservationOrder(actor, resourceReservation);
            },
            () => GoToState("getResource"),
            null);
        CreateState("getResource",
            () => new ExtractFromReservoirOrder(actor, resourceReservation),
            () => {
                resourceReservation = null;
                GoToState("storeContents");
            },
            null);
        CreateState("storeContents",
            () => new StoreCarriedResourceOrder(actor),
            () => GoToState("seekTarget"),
            null);
    }

    public override void Cancel() {
        base.Cancel();
        if (resourceReservation) {
            resourceReservation.Released = true;
            GameObject.Destroy(resourceReservation);
        }
    }
}
