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
        [SerializableMember(1)] private float progress;
        [SerializableMember(2)] private readonly ResourceReservation reservation;

        public override string StatusString => "Extracting resource";

        public ExtractFromReservoirOrder(IOrderable a, ResourceReservation res) {
            reservation = res;
        }

        public override void DoStep(IOrderable orderable) {
            progress += Time.fixedDeltaTime;
            if (progress >= 1.0f) {
                Reservoir reservoir = reservation.source.GetComponent<Reservoir>();
                reservoir.WithdrawReservation(reservation);
                Resource res = orderable.GameController.CreateResourcePile(reservoir.resourceKind, 1);
                orderable.PickupResource(res);
                Completed = true;
            }
        }
    }
}
