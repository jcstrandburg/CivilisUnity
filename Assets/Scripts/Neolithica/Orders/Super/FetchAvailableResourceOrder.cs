using AqlaSerializer;
using Neolithica.MonoBehaviours;
using Neolithica.Orders.Simple;

namespace Neolithica.Orders.Super {
    /// <summary>
    /// Order to fetch the given resource from any available warehouse
    /// </summary>
    [SerializableType]
    public class FetchAvailableResourceOrder : StatefulSuperOrder {
        [SerializableMember(1)] private readonly ResourceKind resourceKind;
        [SerializableMember(2)] private readonly double amount;

        public FetchAvailableResourceOrder(IOrderable actor, ResourceKind resourceKind, double amount) {
            this.resourceKind = resourceKind;
            this.amount = amount;
            GoToState(cGetReservation, actor);
        }

        public override void Initialize(IOrderable orderable) {
            Resource r = orderable.GetCarriedResource();
            if (r != null) {
                if (r.resourceKind == resourceKind) {
                    Completed = true;
                } else {
                    orderable.DropCarriedResource();
                }
            }
        }

        protected override void CreateStates() {
            CreateState(cGetReservation,
                actor => new ReserveWarehouseContentsOrder(resourceKind, amount),
                actor => GoToState(cGotoWarehouse, actor),
                null);
            CreateState(cGotoWarehouse,
                actor => new SimpleMoveOrder(actor, actor.ResourceReservation.source.transform.position, 2.0f),
                actor => GoToState(cWithdraw, actor),
                null);
            CreateState(cWithdraw,
                actor => new SimpleWithdrawOrder(),
                actor => Completed = true,
                null);
        }

        private const string cGetReservation = "getReservation";
        private const string cGotoWarehouse = "gotoWarehouse";
        private const string cWithdraw = "withdraw";
    }
}