using Neolithica.MonoBehaviours;
using Neolithica.Orders.Simple;

namespace Neolithica.Orders.Super {
    /// <summary>
    /// Order to fetch the given resource from any available warehouse
    /// </summary>
    public class FetchAvailableResourceOrder : StatefulSuperOrder {
        Resource.Type resourceType;
        double amount;

        public FetchAvailableResourceOrder(ActorController a, Resource.Type resourceType, double amount) : base(a) {
            this.resourceType = resourceType;
            this.amount = amount;
            GoToState("getReservation");
        }

        public override void Initialize() {
            Resource r = actor.GetCarriedResource();
            if (r != null) {
                if (r.type == resourceType) {
                    this.completed = true;
                } else {
                    actor.DropCarriedResource();
                }
            }
        }

        protected override void CreateStates() {
            CreateState("getReservation",
                () => new ReserveWarehouseContentsOrder(actor, resourceType, amount),
                () => GoToState("gotoWarehouse"),
                null);
            CreateState("gotoWarehouse",
                () => new SimpleMoveOrder(actor, actor.resourceReservation.source.transform.position, 2.0f),
                () => GoToState("withdraw"),
                null);
            CreateState("withdraw",
                () => new SimpleWithdrawOrder(actor),
                () => this.completed = true,
                null);
        }
    }
}