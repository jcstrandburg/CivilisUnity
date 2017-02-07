using Neolithica.MonoBehaviours;
using Neolithica.Orders.Simple;

namespace Neolithica.Orders.Super {
    public class HuntOrder : StatefulSuperOrder {
        Herd herd;

        public HuntOrder(ActorController a, Herd herd) : base(a) {
            this.herd = herd;
            GoToState("seekTarget");
        }

        public override void Initialize() {
            actor.DropCarriedResource();
        }

        protected override void CreateStates() {
            CreateState("seekTarget", 
                () => new SimpleMoveOrder(actor, herd.rabbit.transform.position), 
                () => GoToState("getResource"), 
                null);
            CreateState("getResource", 
                () => new SlaughterHuntedAnimalOrder(actor, herd), 
                () => GoToState("storeResource"), 
                null);
            CreateState("storeResource", 
                () => new StoreCarriedResourceOrder(actor), 
                () => GoToState("seekTarget"), 
                null);
        }
    }
}