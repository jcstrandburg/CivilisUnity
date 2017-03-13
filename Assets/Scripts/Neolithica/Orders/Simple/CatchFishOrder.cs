using AqlaSerializer;
using Neolithica.MonoBehaviours;
using UnityEngine;

namespace Neolithica.Orders.Simple {
    [SerializableType]
    public class CatchFishOrder : BaseOrder {
        [SerializableMember(1)] float progress = 0.0f;

        public CatchFishOrder(ActorController a, NeolithicObject fishingHole) : base(a) {
        }

        public override void DoStep() {
            progress += Time.fixedDeltaTime;
            if (progress >= 1.25f) {
                Resource res = Actor.GameController.CreateResourcePile(ResourceKind.Fish, 1.0);
                Actor.PickupResource(res);
                this.Completed = true;
            }
        }
    }
}