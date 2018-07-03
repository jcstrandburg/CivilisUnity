using AqlaSerializer;
using Assets;
using Neolithica.MonoBehaviours;
using Neolithica.Orders.Simple;
using UnityEngine;

namespace Neolithica.Orders.Super {
    /// <summary>
    /// Testing order to transmute one resource to another
    /// </summary>
    [SerializableType]
    public class TransmuteOrder : StatefulSuperOrder {
        [SerializableMember(1)] private readonly ResourceKind fromResourceKind;
        [SerializableMember(2)] private readonly ResourceKind toResourceKind;
        [SerializableMember(3)] private readonly GameObject target;

        public TransmuteOrder(ActorController actor, GameObject target, ResourceKind fromResourceKind, ResourceKind toResourceKind) {
            this.fromResourceKind = fromResourceKind;
            this.toResourceKind = toResourceKind;
            this.target = target;
            GoToState(cGetSourceMaterial, actor);
        }

        public override void Initialize(IOrderable orderable) {
            Resource r = orderable.GetCarriedResource();
            if (r == null) return;
            if (r.resourceKind == fromResourceKind) {
                GoToState(cGotoWorkspace, orderable);
            }
            else if (r.resourceKind == toResourceKind) {
                GoToState(cStoreProduct, orderable);
            }
            else {
                orderable.DropCarriedResource();
            }
        }

        protected override void CreateStates() {
            CreateState(cGetSourceMaterial,
                actor => new FetchAvailableResourceOrder(actor, fromResourceKind, 1),
                actor => GoToState(cGotoWorkspace, actor),
                null);
            CreateState(cGotoWorkspace,
                actor => new SimpleMoveOrder(actor, target.transform.position, 2.0f),
                actor => GoToState(cDoTransmute, actor),
                null);
            CreateState(cDoTransmute,
                actor => new ConvertResourceOrder(actor, fromResourceKind, toResourceKind),
                actor => GoToState(cStoreProduct, actor),
                null);
            CreateState(cStoreProduct,
                actor => new StoreCarriedResourceOrder(actor),
                actor => GoToState(cGetSourceMaterial, actor),
                null);
        }

        private const string cGetSourceMaterial = "getSourceMaterial";
        private const string cGotoWorkspace = "gotoWorkspace";
        private const string cDoTransmute = "doTransmute";
        private const string cStoreProduct = "storeProduct";
    }
}