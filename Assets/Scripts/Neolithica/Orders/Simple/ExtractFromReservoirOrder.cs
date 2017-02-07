using Neolithica.MonoBehaviours;
using Neolithica.MonoBehaviours.Reservations;
using UnityEngine;

namespace Neolithica.Orders.Simple {
    /// <summary>
    /// Order to extract resources from the given target object
    /// </summary>
    public class ExtractFromReservoirOrder : BaseOrder {
        float progress;
        NeolithicObject target;
        ResourceReservation reservation;

        public ExtractFromReservoirOrder(ActorController a, ResourceReservation res) : base(a) {
            a.GetComponent<NeolithicObject>().statusString = "Extracting resource";
            reservation = res;
        }

        public override void DoStep() {
            progress += Time.fixedDeltaTime;
            if (progress >= 1.0f) {
                Reservoir reservoir = reservation.source.GetComponent<Reservoir>();
                reservoir.WithdrawReservation(reservation);
                Resource res = actor.GameController.CreateResourcePile(reservoir.resourceType, 1);
                actor.PickupResource(res);
                completed = true;
            }
        }
    }
}
