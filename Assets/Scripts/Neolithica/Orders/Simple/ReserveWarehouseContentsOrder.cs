using AqlaSerializer;
using Neolithica.MonoBehaviours;

namespace Neolithica.Orders.Simple {
    /// <summary>
    /// Order to reserve the given resrouces from any available warehouse
    /// </summary>
    [SerializableType]
    public class ReserveWarehouseContentsOrder : BaseOrder {
        [SerializableMember(1)]
        private ResourceKind resourceResourceKind;
        [SerializableMember(2)]
        private double amount;

        public ReserveWarehouseContentsOrder(ActorController a, ResourceKind resourceResourceKind, double amount) : base() {
            this.resourceResourceKind = resourceResourceKind;
            this.amount = amount;
        }

        public override void DoStep(ActorController actor) {
            if (actor.GameController.ReserveWarehouseResources(actor, resourceResourceKind, amount)) {
                Completed = true;
            }
        }
    }
}
