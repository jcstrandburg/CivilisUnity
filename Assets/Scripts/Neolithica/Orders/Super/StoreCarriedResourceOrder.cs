using AqlaSerializer;
using Neolithica.MonoBehaviours;
using Neolithica.Orders.Simple;

namespace Neolithica.Orders.Super {
    /// <summary>
    /// Super order to find, seek, and utilize storage for the currently carried resource
    /// </summary>
    [SerializableType]
    public class StoreCarriedResourceOrder : StatefulSuperOrder {
        public StoreCarriedResourceOrder(ActorController a) : base(a) {
            GoToState("getReservation");
        }

        protected override void CreateStates() {
            CreateState("getReservation",
                () => new ReserveStorageOrder(Actor),
                () => GoToState("seekStorage"),
                () => {
                    //Debug.Log("Couldn't get reservation, dumping!");
                    GoToState("dump");
                });
            CreateState("dump",
                () => new DumpCarriedResourceOrder(Actor),
                () => this.Completed = true,
                null);
            CreateState("seekStorage",
                () => new SimpleMoveOrder(Actor, Actor.storageReservation.warehouse.transform.position, 2.0f),
                () => GoToState("reservationWait"),
                null);
            CreateState("reservationWait",
                () => new WaitForReservationOrder(Actor, Actor.storageReservation),
                () => GoToState("deposit"),
                null);
            CreateState("deposit",
                () => new StoreReservationOrder(Actor, Actor.storageReservation),
                () => this.Completed = true,
                null);
        }
    }
}
