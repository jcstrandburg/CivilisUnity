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
            GoToState(cGetReservation, actor);
        }

        protected override void CreateStates() {
            CreateState(cGetReservation,
                actor => new ReserveStorageOrder(actor),
                actor => GoToState(cSeekStorage, actor),
                actor => {
                    GoToState(cDump, actor);
                });
            CreateState(cDump,
                actor => new DumpCarriedResourceOrder(actor),
                actor => this.Completed = true,
                null);
            CreateState(cSeekStorage,
                actor => new SimpleMoveOrder(actor, actor.storageReservation.warehouse.transform.position, 2.0f),
                actor => GoToState(cReservationWait, actor),
                null);
            CreateState(cReservationWait,
                actor => new WaitForReservationOrder(actor, actor.storageReservation),
                actor => GoToState(cDeposit, actor),
                null);
            CreateState(cDeposit,
                actor => new StoreReservationOrder(actor, actor.storageReservation),
                actor => this.Completed = true,
                null);
        }

        private const string cGetReservation = "getReservation";
        private const string cDump = "dump";
        private const string cSeekStorage = "seekStorage";
        private const string cReservationWait = "reservationWait";
        private const string cDeposit = "deposit";
    }
}
