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
            GoToState("getReservation", actor);
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
            CreateState("getReservation",
                actor => new ReserveWarehouseContentsOrder(actor, resourceResourceKind, amount),
                actor => GoToState("gotoWarehouse", actor),
                null);
            CreateState("gotoWarehouse",
                actor => new SimpleMoveOrder(actor, actor.resourceReservation.source.transform.position, 2.0f),
                actor => GoToState("withdraw", actor),
                null);
            CreateState("withdraw",
                actor => new SimpleWithdrawOrder(actor),
                actor => this.Completed = true,
                null);
        }
    }
}