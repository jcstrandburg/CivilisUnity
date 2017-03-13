using AqlaSerializer;
using Neolithica.MonoBehaviours;
using Neolithica.Orders.Simple;

namespace Neolithica.Orders.Super {
    [SerializableType]
    public class HuntOrder : StatefulSuperOrder {
        [SerializableMember(1)]
        private Herd herd;

        public HuntOrder(ActorController a, Herd herd) : base(a) {
            this.herd = herd;
            GoToState("seekTarget");
        }

        public override void Initialize() {
            Actor.DropCarriedResource();
        }

        protected override void CreateStates() {
            CreateState("seekTarget", 
                () => new SimpleMoveOrder(Actor, herd.rabbit.transform.position), 
                () => GoToState("getResource"), 
                null);
            CreateState("getResource", 
                () => new SlaughterHuntedAnimalOrder(Actor, herd), 
                () => GoToState("storeResource"), 
                null);
            CreateState("storeResource", 
                () => new StoreCarriedResourceOrder(Actor), 
                () => GoToState("seekTarget"), 
                null);
        }
    }
}