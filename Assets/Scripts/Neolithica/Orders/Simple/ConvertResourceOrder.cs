using AqlaSerializer;
using Neolithica.MonoBehaviours;
using UnityEngine;

namespace Neolithica.Orders.Simple {
    /// <summary>
    /// Simple order to convert a carried resource of one type to another type
    /// </summary>
    [SerializableType]
    public class ConvertResourceOrder : BaseOrder {
        [SerializableMember(1)] private readonly Resource sourceResource;
        [SerializableMember(2)] private readonly ResourceKind toResourceKind;

        public ConvertResourceOrder(ActorController a, ResourceKind fromResourceKind, ResourceKind toResourceKind) : base() {
            Resource r = a.GetCarriedResource();
            if (r.resourceKind != fromResourceKind) {
                Debug.Log($"Actor does not have resource {fromResourceKind} to convert");
                Failed = true;
            }
            sourceResource = r;
            this.toResourceKind = toResourceKind;
        }

        public override void DoStep(ActorController actor) {
            Resource newResource = actor.GameController.CreateResourcePile(toResourceKind, 1);
            newResource.amount = sourceResource.amount;
            actor.PickupResource(newResource);
            sourceResource.transform.SetParent(null);
            Object.Destroy(sourceResource.gameObject);
            Completed = true;
        }
    }
}