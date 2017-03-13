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

        public TransmuteOrder(ActorController a, NeolithicObject target, ResourceKind fromResourceKind, ResourceKind toResourceKind) : base(a) {
            this.fromResourceKind = fromResourceKind;
            this.toResourceKind = toResourceKind;
            this.target = target;
            GoToState("getSourceMaterial");
        }

        public override void Initialize() {
            Resource r = Actor.GetCarriedResource();
            if (r == null) return;
            if (r.resourceKind == fromResourceKind) {
                GoToState("gotoWorkspace");
            }
            else if (r.resourceKind == toResourceKind) {
                GoToState("storeProduct");
            }
            else {
                Actor.DropCarriedResource();
            }
        }

        protected override void CreateStates() {
            CreateState("getSourceMaterial",
                () => new FetchAvailableResourceOrder(Actor, fromResourceKind, 1),
                () => GoToState("gotoWorkspace"),
                null);
            CreateState("gotoWorkspace",
                () => new SimpleMoveOrder(Actor, target.transform.position, 2.0f),
                () => GoToState("doTransmute"),
                null);
            CreateState("doTransmute",
                () => new ConvertResourceOrder(Actor, fromResourceKind, toResourceKind),
                () => GoToState("storeProduct"),
                null);
            CreateState("storeProduct",
                () => new StoreCarriedResourceOrder(Actor),
                () => GoToState("getSourceMaterial"),
                null);
        }
    }
}