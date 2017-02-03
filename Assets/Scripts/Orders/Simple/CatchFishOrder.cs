using UnityEngine;

public class CatchFishOrder : BaseOrder {
    float progress = 0.0f;

    public CatchFishOrder(ActorController a, NeolithicObject fishingHole) : base(a) {
    }

    public override void DoStep() {
        progress += Time.fixedDeltaTime;
        if (progress >= 1.25f) {
            Resource res = actor.GameController.CreateResourcePile(Resource.Type.Fish, 1.0);
            actor.PickupResource(res);
            this.completed = true;
        }
    }
}