using AqlaSerializer;
using Neolithica.MonoBehaviours;
using Neolithica.Orders.Simple;

namespace Neolithica.Orders.Super {
    [SerializableType]
    public class FishOrder : StatefulSuperOrder {
        [SerializableMember(1)]
        private NeolithicObject target;

        public FishOrder(ActorController actor, NeolithicObject target) {
            this.target = target;
            GoToState("seekTarget", actor);
        }

        public override void Initialize(ActorController actor) {
            actor.DropCarriedResource();
        }

        protected override void CreateStates() {
            CreateState(
                "seekTarget",
                actor => new SimpleMoveOrder(actor, target.transform.position, 20.0f),
                actor => GoToState("getResource", actor), 
                null);
            CreateState(
                "getResource",
                actor => new CatchFishOrder(actor, target),
                actor => GoToState("storeResource", actor),
                null);
            CreateState(
                "storeResource",
                actor => new StoreCarriedResourceOrder(actor),
                actor => GoToState("seekTarget", actor),
                null);
        }
    }
}
