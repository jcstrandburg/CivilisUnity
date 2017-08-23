using AqlaSerializer;
using Neolithica.MonoBehaviours;
using Neolithica.Orders.Simple;

namespace Neolithica.Orders.Super {
    /// <summary>
    /// Order to fetch the given resource from any available warehouse
    /// </summary>
    [SerializableType]
    public class FetchAvailableResourceOrder : StatefulSuperOrder {
        [SerializableMember(1)]
        private ResourceKind resourceResourceKind;
        [SerializableMember(2)]
        private double amount;

        public FetchAvailableResourceOrder(ActorController actor, ResourceKind resourceResourceKind, double amount) {
            this.resourceResourceKind = resourceResourceKind;
            this.amount = amount;
            GoToState(cGetReservation, actor);
        }

        public override void Initialize(ActorController actor) {
            Resource r = actor.GetCarriedResource();
            if (r != null) {
                if (r.resourceKind == resourceResourceKind) {
                    this.Completed = true;
                } else {
                    actor.DropCarriedResource();
                }
            }
        }

        protected override void CreateStates() {
            CreateState(cGetReservation,
                actor => new ReserveWarehouseContentsOrder(actor, resourceResourceKind, amount),
                actor => GoToState(cGotoWarehouse, actor),
                null);
            CreateState(cGotoWarehouse,
                actor => new SimpleMoveOrder(actor, actor.resourceReservation.source.transform.position, 2.0f),
                actor => GoToState(cWithdraw, actor),
                null);
            CreateState(cWithdraw,
                actor => new SimpleWithdrawOrder(actor),
                actor => this.Completed = true,
                null);
        }

        private const string cGetReservation = "getReservation";
        private const string cGotoWarehouse = "gotoWarehouse";
        private const string cWithdraw = "withdraw";
    }
}