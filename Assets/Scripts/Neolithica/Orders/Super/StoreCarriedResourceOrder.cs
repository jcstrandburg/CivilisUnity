using Neolithica.MonoBehaviours;
using Neolithica.Orders.Simple;

namespace Neolithica.Orders.Super {
    /// <summary>
    /// Super order to find, seek, and utilize storage for the currently carried resource
    /// </summary>
    public class StoreCarriedResourceOrder : StatefulSuperOrder {
        public StoreCarriedResourceOrder(ActorController a) : base(a) {
            GoToState("getReservation");
        }

        protected override void CreateStates() {
            CreateState("getReservation",
                () => new ReserveStorageOrder(actor),
                () => GoToState("seekStorage"),
                () => {
                    //Debug.Log("Couldn't get reservation, dumping!");
                    GoToState("dump");
                });
            CreateState("dump",
                () => new DumpCarriedResourceOrder(actor),
                () => this.completed = true,
                null);
            CreateState("seekStorage",
                () => new SimpleMoveOrder(actor, actor.storageReservation.warehouse.transform.position, 2.0f),
                () => GoToState("reservationWait"),
                null);
            CreateState("reservationWait",
                () => new WaitForReservationOrder(actor, actor.storageReservation),
                () => GoToState("deposit"),
                null);
            CreateState("deposit",
                () => new StoreReservationOrder(actor, actor.storageReservation),
                () => this.completed = true,
                null);
        }
    }
}
