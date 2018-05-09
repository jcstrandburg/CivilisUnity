using AqlaSerializer;
using Neolithica.MonoBehaviours;
using Neolithica.Orders.Simple;
using UnityEngine;

namespace Neolithica.Orders.Super {
    [SerializableType]
    public class FishOrder : StatefulSuperOrder {
        [SerializableMember(1)] private readonly GameObject target;

        public FishOrder(ActorController actor, GameObject target) {
            this.target = target;
            GoToState(cSeekTarget, actor);
        }

        public override void Initialize(ActorController actor) {
            actor.DropCarriedResource();
        }

        protected override void CreateStates() {
            CreateState(
                cSeekTarget,
                actor => new SimpleMoveOrder(actor, target.transform.position, 20.0f),
                actor => GoToState(cGetResource, actor), 
                null);
            CreateState(
                cGetResource,
                actor => new CatchFishOrder(),
                actor => GoToState(cStoreResource, actor),
                null);
            CreateState(
                cStoreResource,
                actor => new StoreCarriedResourceOrder(actor),
                actor => GoToState(cSeekTarget, actor),
                null);
        }

        private const string cSeekTarget = "cSeekTarget";
        private const string cGetResource = "getResource";
        private const string cStoreResource = "storeResource";
    }
}
