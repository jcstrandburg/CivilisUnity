using Neolithica.MonoBehaviours;
using Neolithica.Orders.Simple;

namespace Neolithica.Orders.Super {
    /// <summary>
    /// Testing order to transmute one resource to another
    /// </summary>
    public class TransmuteOrder : StatefulSuperOrder {
        Resource.Type fromType;
        Resource.Type toType;
        NeolithicObject target;

        public TransmuteOrder(ActorController a, NeolithicObject target, Resource.Type fromType, Resource.Type toType) : base(a) {
            this.fromType = fromType;
            this.toType = toType;
            this.target = target;
            GoToState("getSourceMaterial");
        }

        public override void Initialize() {
            Resource r = actor.GetCarriedResource();
            if (r == null) return;
            if (r.type == fromType) {
                GoToState("gotoWorkspace");
            }
            else if (r.type == toType) {
                GoToState("storeProduct");
            }
            else {
                actor.DropCarriedResource();
            }
        }

        protected override void CreateStates() {
            CreateState("getSourceMaterial",
                () => new FetchAvailableResourceOrder(actor, fromType, 1),
                () => GoToState("gotoWorkspace"),
                null);
            CreateState("gotoWorkspace",
                () => new SimpleMoveOrder(actor, target.transform.position, 2.0f),
                () => GoToState("doTransmute"),
                null);
            CreateState("doTransmute",
                () => new ConvertResourceOrder(actor, fromType, toType),
                () => GoToState("storeProduct"),
                null);
            CreateState("storeProduct",
                () => new StoreCarriedResourceOrder(actor),
                () => GoToState("getSourceMaterial"),
                null);
        }
    }
}