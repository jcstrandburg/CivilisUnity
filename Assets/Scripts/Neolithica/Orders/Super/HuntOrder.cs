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
            GoToState(cSeekTarget, actor);
        }

        public override void Initialize(ActorController actor) {
            actor.DropCarriedResource();
        }

        protected override void CreateStates() {
            CreateState(cSeekTarget,
                actor => new SimpleMoveOrder(actor, herd.rabbit.transform.position),
                actor => GoToState(cGetResource, actor), 
                null);
            CreateState(cGetResource,
                actor => new SlaughterHuntedAnimalOrder(actor, herd),
                actor => GoToState(cStoreResource, actor), 
                null);
            CreateState(cStoreResource,
                actor => new StoreCarriedResourceOrder(actor),
                actor => GoToState(cSeekTarget, actor), 
                null);
        }

        private const string cSeekTarget = "seekTarget";
        private const string cGetResource = "getResource";
        private const string cStoreResource = "storeResource";
    }
}