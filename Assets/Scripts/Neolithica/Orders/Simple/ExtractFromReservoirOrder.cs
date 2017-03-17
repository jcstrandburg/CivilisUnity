using AqlaSerializer;
using Neolithica.MonoBehaviours;
using Neolithica.MonoBehaviours.Reservations;
using UnityEngine;

namespace Neolithica.Orders.Simple {
    /// <summary>
    /// Order to extract resources from the given target object
    /// </summary>
    [SerializableType]
    public class ExtractFromReservoirOrder : BaseOrder {
        [SerializableMember(1)]
        private float progress;
        [SerializableMember(2)]
        private readonly ResourceReservation reservation;

        public ExtractFromReservoirOrder(ActorController a, ResourceReservation res) : base() {
            a.GetComponent<NeolithicObject>().statusString = "Extracting resource";
            reservation = res;
        }

        public override void DoStep(ActorController actor) {
            progress += Time.fixedDeltaTime;
            if (progress >= 1.0f) {
                Reservoir reservoir = reservation.source.GetComponent<Reservoir>();
                reservoir.WithdrawReservation(reservation);
                Resource res = actor.GameController.CreateResourcePile(reservoir.resourceResourceKind, 1);
                actor.PickupResource(res);
                Completed = true;
            }
        }
    }
}
