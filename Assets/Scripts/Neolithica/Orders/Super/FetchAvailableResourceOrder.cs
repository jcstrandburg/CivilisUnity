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

        public FetchAvailableResourceOrder(ActorController a, ResourceKind resourceResourceKind, double amount) : base(a) {
            this.resourceResourceKind = resourceResourceKind;
            this.amount = amount;
            GoToState("getReservation");
        }

        public override void Initialize() {
            Resource r = Actor.GetCarriedResource();
            if (r != null) {
                if (r.resourceKind == resourceResourceKind) {
                    this.Completed = true;
                } else {
                    Actor.DropCarriedResource();
                }
            }
        }

        protected override void CreateStates() {
            CreateState("getReservation",
                () => new ReserveWarehouseContentsOrder(Actor, resourceResourceKind, amount),
                () => GoToState("gotoWarehouse"),
                null);
            CreateState("gotoWarehouse",
                () => new SimpleMoveOrder(Actor, Actor.resourceReservation.source.transform.position, 2.0f),
                () => GoToState("withdraw"),
                null);
            CreateState("withdraw",
                () => new SimpleWithdrawOrder(Actor),
                () => this.Completed = true,
                null);
        }
    }
}