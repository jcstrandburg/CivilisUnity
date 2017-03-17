using AqlaSerializer;
using Neolithica.MonoBehaviours;
using Neolithica.Orders.Simple;

namespace Neolithica.Orders.Super {
    /// <summary>
    /// Testing order to transmute one resource to another
    /// </summary>
    [SerializableType]
    public class TransmuteOrder : StatefulSuperOrder {
        [SerializableMember(1)]
        private ResourceKind fromResourceKind;
        [SerializableMember(2)]
        private ResourceKind toResourceKind;
        [SerializableMember(3)]
        private NeolithicObject target;

        public TransmuteOrder(ActorController actor, NeolithicObject target, ResourceKind fromResourceKind, ResourceKind toResourceKind) : base(actor) {
            this.fromResourceKind = fromResourceKind;
            this.toResourceKind = toResourceKind;
            this.target = target;
            GoToState("getSourceMaterial", actor);
        }

        public override void Initialize(ActorController actor) {
            Resource r = actor.GetCarriedResource();
            if (r == null) return;
            if (r.resourceKind == fromResourceKind) {
                GoToState("gotoWorkspace", actor);
            }
            else if (r.resourceKind == toResourceKind) {
                GoToState("storeProduct", actor);
            }
            else {
                actor.DropCarriedResource();
            }
        }

        protected override void CreateStates() {
            CreateState("getSourceMaterial",
                actor => new FetchAvailableResourceOrder(actor, fromResourceKind, 1),
                actor => GoToState("gotoWorkspace", actor),
                null);
            CreateState("gotoWorkspace",
                actor => new SimpleMoveOrder(actor, target.transform.position, 2.0f),
                actor => GoToState("doTransmute", actor),
                null);
            CreateState("doTransmute",
                actor => new ConvertResourceOrder(actor, fromResourceKind, toResourceKind),
                actor => GoToState("storeProduct", actor),
                null);
            CreateState("storeProduct",
                actor => new StoreCarriedResourceOrder(actor),
                actor => GoToState("getSourceMaterial", actor),
                null);
        }
    }
}