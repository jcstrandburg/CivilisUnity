using AqlaSerializer;
using Neolithica.MonoBehaviours;
using Neolithica.Orders.Simple;

namespace Neolithica.Orders.Super {
    [SerializableType]
    public class FishOrder : StatefulSuperOrder {
        [SerializableMember(1)]
        private NeolithicObject target;

        public FishOrder(ActorController a, NeolithicObject target) : base(a) {
            this.target = target;
            GoToState("seekTarget");
        }

        public override void Initialize() {
            Actor.DropCarriedResource();
        }

        protected override void CreateStates() {
            CreateState(
                "seekTarget", 
                () => new SimpleMoveOrder(Actor, target.transform.position, 20.0f), 
                () => GoToState("getResource"), 
                null);
            CreateState(
                "getResource", 
                () => new CatchFishOrder(Actor, target),
                () => GoToState("storeResource"),
                null);
            CreateState(
                "storeResource",
                () => new StoreCarriedResourceOrder(Actor),
                () => GoToState("seekTarget"),
                null);
        }
    }
}
