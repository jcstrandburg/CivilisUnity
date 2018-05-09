using AqlaSerializer;
using Neolithica.MonoBehaviours;
using Neolithica.MonoBehaviours.Reservations;
using UnityEngine;

namespace Neolithica.Orders.Simple {
    /// <summary>
    /// Order to store the resources for the given StorageReservation in any available warehouse
    /// </summary>
    [SerializableType]
    public class StoreReservationOrder : BaseOrder {
        [SerializableMember(1)] private readonly StorageReservation res;

        public StoreReservationOrder(StorageReservation r) {
            res = r;
        }

        public override void DoStep(ActorController actor) {
            if (actor.MoveTowards(res.warehouse.transform.position)) {
                res.warehouse.DepositReservation(res);
                Object.Destroy(actor.GetCarriedResource(res.resourceKind).gameObject);
                res.Released = true;
                Completed = true;
            }
        }
    }
}
