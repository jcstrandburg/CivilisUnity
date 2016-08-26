using UnityEngine;
using System.Collections;

/// <summary>
/// Testing order to transmute one resource to another
/// </summary>
public class TransmuteOrder : StatefulSuperOrder {
    string fromTag;
    string toTag;
    NeolithicObject target;

    public TransmuteOrder(ActorController a, NeolithicObject target, string fromTag, string toTag) : base(a) {
        this.fromTag = fromTag;
        this.toTag = toTag;
        this.target = target;
        GoToState("getSourceMaterial");
    }

    public override void Initialize() {
        Resource r = actor.GetCarriedResource();
        if (r != null) {
            if (r.typeTag == fromTag) {
                GoToState("gotoWorkspace");
            }
            else if (r.typeTag == toTag) {
                GoToState("storeProduct");
            }
            else {
                actor.DropCarriedResource();
            }
        }
    }

    protected override void CreateStates() {
        CreateState("getSourceMaterial",
            () => new FetchAvailableResourceOrder(actor, fromTag, 1),
            () => GoToState("gotoWorkspace"),
            null);
        CreateState("gotoWorkspace",
            () => new SimpleMoveOrder(actor, target.transform.position, 2.0f),
            () => GoToState("doTransmute"),
            null);
        CreateState("doTransmute",
            () => new ConvertResourceOrder(actor, fromTag, toTag),
            () => GoToState("storeProduct"),
            null);
        CreateState("storeProduct",
            () => new StoreCarriedResourceOrder(actor),
            () => GoToState("getSourceMaterial"),
            null);
    }
}