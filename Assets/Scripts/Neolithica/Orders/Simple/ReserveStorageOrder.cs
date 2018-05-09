using AqlaSerializer;
using Neolithica.MonoBehaviours;
using UnityEngine;

namespace Neolithica.Orders.Simple {
    /// <summary>
    /// Order to reserve storage for the current carried resource in any available warehouse
    /// </summary>
    [SerializableType]
    public class ReserveStorageOrder : BaseOrder {
        [SerializableMember(1)] private readonly Resource resource;

        public ReserveStorageOrder(ActorController a) {
            resource = a.GetCarriedResource();
            if (!resource) {
                Failed = true;
            }
        }

        public override void DoStep(ActorController actor) {
            ResourceKind resourceKind = resource.resourceKind;
            double amount = resource.amount;

            if (actor.GameController.ReserveStorage(actor, resourceKind, amount))
                Completed = true;
            else
                Failed = true;
        }
    }
}