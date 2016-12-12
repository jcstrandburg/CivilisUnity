using UnityEngine;
using System.Collections;

/// <summary>
/// Simple order to seek, reserve, and extract resources from the given target
/// </summary>
public class HarvestFromReservoirOrder : StatefulSuperOrder {
    NeolithicObject targetObj;
    Reservoir reservoir;
    ResourceReservation resourceReservation;

    public HarvestFromReservoirOrder(ActorController a, NeolithicObject target) : base(a) {
        targetObj = target;
        reservoir = target.GetComponent<Reservoir>();
        GoToState("seekTarget");
    }

    public override void Initialize() {
        Resource r = actor.GetCarriedResource();
        if (r != null && r.type == reservoir.resourceType) {
            GoToState("storeContents");
        } else {
            actor.DropCarriedResource();
        }
    }

    protected override void CreateStates() {
        CreateState("seekTarget",
            () => new SimpleMoveOrder(
                    actor, 
                    actor.GameController.SnapToGround(targetObj.transform.position)),
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
        }
    }
}
