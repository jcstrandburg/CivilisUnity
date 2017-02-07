using Neolithica.MonoBehaviours;
using Neolithica.Orders.Simple;

namespace Neolithica.Orders.Super {
    public class FishOrder : StatefulSuperOrder {
        NeolithicObject target;

        public FishOrder(ActorController a, NeolithicObject target) : base(a) {
            this.target = target;
            GoToState("seekTarget");
        }

        public override void Initialize() {
            actor.DropCarriedResource();
        }

        protected override void CreateStates() {
            CreateState(
                "seekTarget", 
                () => new SimpleMoveOrder(actor, target.transform.position, 20.0f), 
                () => GoToState("getResource"), 
                null);
            CreateState(
                "getResource", 
                () => new CatchFishOrder(actor, target),
                () => GoToState("storeResource"),
                null);
            CreateState(
                "storeResource",
                () => new StoreCarriedResourceOrder(actor),
                () => GoToState("seekTarget"),
                null);
        }
    }
}
