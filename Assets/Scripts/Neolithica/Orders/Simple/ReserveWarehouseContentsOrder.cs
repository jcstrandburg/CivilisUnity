using AqlaSerializer;
using Neolithica.MonoBehaviours;

namespace Neolithica.Orders.Simple {
    /// <summary>
    /// Order to reserve the given resrouces from any available warehouse
    /// </summary>
    [SerializableType]
    public class ReserveWarehouseContentsOrder : BaseOrder {
        [SerializableMember(1)] private readonly ResourceKind resourceKind;
        [SerializableMember(2)] private readonly double amount;

        public ReserveWarehouseContentsOrder(ResourceKind resourceKind, double amount) {
            this.resourceKind = resourceKind;
            this.amount = amount;
        }

        public override void DoStep(IOrderable orderable) {
            if (orderable.GameController.ReserveWarehouseResources(orderable, resourceKind, amount)) {
                Completed = true;
            }
        }
    }
}
