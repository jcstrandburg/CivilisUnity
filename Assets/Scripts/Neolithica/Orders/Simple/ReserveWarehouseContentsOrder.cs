using AqlaSerializer;
using Neolithica.MonoBehaviours;

namespace Neolithica.Orders.Simple {
    /// <summary>
    /// Order to reserve the given resrouces from any available warehouse
    /// </summary>
    [SerializableType]
    public class ReserveWarehouseContentsOrder : BaseOrder {
        [SerializableMember(1)]
        private ResourceKind resourceKind;
        [SerializableMember(2)]
        private double amount;

        public ReserveWarehouseContentsOrder(ActorController a, ResourceKind resourceKind, double amount) : base() {
            this.resourceKind = resourceKind;
            this.amount = amount;
        }

        public override void DoStep(ActorController actor) {
            if (actor.GameController.ReserveWarehouseResources(actor, resourceKind, amount)) {
                Completed = true;
            }
        }
    }
}
