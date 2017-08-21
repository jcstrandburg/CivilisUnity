using AqlaSerializer;
using Neolithica.MonoBehaviours;
using Neolithica.Orders.Simple;

namespace Neolithica.Orders.Super {
    [SerializableType]
    public class HuntOrder : StatefulSuperOrder {
        [SerializableMember(1)]
        private Herd herd;

        public HuntOrder(ActorController actor, Herd herd) {
            this.herd = herd;
            GoToState("seekTarget", actor);
        }

        public override void Initialize(ActorController actor) {
            actor.DropCarriedResource();
        }

        protected override void CreateStates() {
            CreateState("seekTarget",
                actor => new SimpleMoveOrder(actor, herd.rabbit.transform.position),
                actor => GoToState("getResource", actor), 
                null);
            CreateState("getResource",
                actor => new SlaughterHuntedAnimalOrder(actor, herd),
                actor => GoToState("storeResource", actor), 
                null);
            CreateState("storeResource",
                actor => new StoreCarriedResourceOrder(actor),
                actor => GoToState("seekTarget", actor), 
                null);
        }
    }
}