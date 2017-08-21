using AqlaSerializer;
using Neolithica.MonoBehaviours;
using Neolithica.Orders.Simple;

namespace Neolithica.Orders.Super {
    /// <summary>
    /// Super order to find, seek, and utilize storage for the currently carried resource
    /// </summary>
    [SerializableType]
    public class StoreCarriedResourceOrder : StatefulSuperOrder {
        public StoreCarriedResourceOrder(ActorController actor) {
            GoToState("getReservation", actor);
        }

        protected override void CreateStates() {
            CreateState("getReservation",
                actor => new ReserveStorageOrder(actor),
                actor => GoToState("seekStorage", actor),
                actor => {
                    GoToState("dump", actor);
                });
            CreateState("dump",
                actor => new DumpCarriedResourceOrder(actor),
                actor => this.Completed = true,
                null);
            CreateState("seekStorage",
                actor => new SimpleMoveOrder(actor, actor.storageReservation.warehouse.transform.position, 2.0f),
                actor => GoToState("reservationWait", actor),
                null);
            CreateState("reservationWait",
                actor => new WaitForReservationOrder(actor, actor.storageReservation),
                actor => GoToState("deposit", actor),
                null);
            CreateState("deposit",
                actor => new StoreReservationOrder(actor, actor.storageReservation),
                actor => this.Completed = true,
                null);
        }
    }
}
