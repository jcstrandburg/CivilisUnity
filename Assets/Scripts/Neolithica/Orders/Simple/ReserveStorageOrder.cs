using AqlaSerializer;
using Neolithica.MonoBehaviours;
using UnityEngine;

namespace Neolithica.Orders.Simple {
    /// <summary>
    /// Order to reserve storage for the current carried resource in any available warehouse
    /// </summary>
    [SerializableType]
    public class ReserveStorageOrder : BaseOrder {
        [SerializableMember(1)]
        private Resource resource;

        public ReserveStorageOrder(ActorController a) : base(a) {
            resource = a.GetCarriedResource();
            if (!resource) {
                Failed = true;
            }
        }

        public override void DoStep() {
            ResourceKind resourceKind = resource.resourceKind;
            double amount = resource.amount;

            if (Actor.GameController.ReserveStorage(Actor, resourceKind, amount)) {
                Debug.Log("Reserved storage");
                Completed = true;
            }
            else {
                Failed = true;
            }
        }
    }
}