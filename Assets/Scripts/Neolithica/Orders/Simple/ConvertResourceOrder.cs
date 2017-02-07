using Neolithica.MonoBehaviours;
using UnityEngine;

namespace Neolithica.Orders.Simple {
    /// <summary>
    /// Simple order to convert a carried resource of one type to another type
    /// </summary>
    public class ConvertResourceOrder : BaseOrder {
        Resource sourceResource;
        Resource.Type toType;

        public ConvertResourceOrder(ActorController a, Resource.Type fromType, Resource.Type toType) : base(a) {
            Resource r = a.GetCarriedResource();
            if (r.type != fromType) {
                Debug.Log("Actor does not have resource " + fromType + " to convert");
                this.failed = true;
            }
            sourceResource = r;
            this.toType = toType;
        }

        public override void DoStep() {
            Resource newResource = actor.GameController.CreateResourcePile(toType, 1);
            newResource.amount = sourceResource.amount;
            actor.PickupResource(newResource);
            sourceResource.transform.SetParent(null);
            UnityEngine.Object.Destroy(sourceResource.gameObject);
            this.completed = true;
        }
    }
}