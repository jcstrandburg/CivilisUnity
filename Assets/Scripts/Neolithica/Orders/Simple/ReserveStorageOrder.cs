using AqlaSerializer;
using Assets;
using Neolithica.MonoBehaviours;
using UnityEngine;

namespace Neolithica.Orders.Simple {
    /// <summary>
    /// Order to reserve storage for the current carried resource in any available warehouse
    /// </summary>
    [SerializableType]
    public class ReserveStorageOrder : BaseOrder {
        [SerializableMember(1)] private readonly Resource resource;

        public ReserveStorageOrder(IOrderable a) {
            resource = a.GetCarriedResource();
            if (!resource) {
                Failed = true;
            }
        }

        public override void DoStep(IOrderable orderable) {
            ResourceKind resourceKind = resource.resourceKind;
            double amount = resource.amount;

            if (orderable.GameController.ReserveStorage(orderable, resourceKind, amount))
                Completed = true;
            else
                Failed = true;
        }
    }
}