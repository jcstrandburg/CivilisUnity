using AqlaSerializer;
using Neolithica.MonoBehaviours;
using UnityEngine;

namespace Neolithica.Orders.Simple {
    [SerializableType]
    public class CatchFishOrder : BaseOrder {
        [SerializableMember(1)] float progress;

        public override void DoStep(ActorController actor) {
            progress += Time.fixedDeltaTime;
            if (progress >= 1.25f) {
                Resource res = actor.GameController.CreateResourcePile(ResourceKind.Fish, 1.0);
                actor.PickupResource(res);
                Completed = true;
            }
        }
    }
}