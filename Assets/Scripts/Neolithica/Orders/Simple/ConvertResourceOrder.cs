using AqlaSerializer;
using Neolithica.MonoBehaviours;
using UnityEngine;

namespace Neolithica.Orders.Simple {
    /// <summary>
    /// Simple order to convert a carried resource of one type to another type
    /// </summary>
    [SerializableType]
    public class ConvertResourceOrder : BaseOrder {
        [SerializableMember(1)] private Resource sourceResource;
        [SerializableMember(2)] private ResourceKind toResourceKind;

        public ConvertResourceOrder(ActorController a, ResourceKind fromResourceKind, ResourceKind toResourceKind) : base(a) {
            Resource r = a.GetCarriedResource();
            if (r.resourceKind != fromResourceKind) {
                Debug.Log("Actor does not have resource " + fromResourceKind + " to convert");
                this.Failed = true;
            }
            sourceResource = r;
            this.toResourceKind = toResourceKind;
        }

        public override void DoStep() {
            Resource newResource = Actor.GameController.CreateResourcePile(toResourceKind, 1);
            newResource.amount = sourceResource.amount;
            Actor.PickupResource(newResource);
            sourceResource.transform.SetParent(null);
            UnityEngine.Object.Destroy(sourceResource.gameObject);
            this.Completed = true;
        }
    }
}