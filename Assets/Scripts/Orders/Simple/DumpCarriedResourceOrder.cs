using UnityEngine;
using System;

/// <summary>
/// Order to dump the currently carried resource on the ground
/// </summary>
public class DumpCarriedResourceOrder : BaseOrder {
    GameObject dump;
    Vector3 target;

    public DumpCarriedResourceOrder(ActorController a) : base(a) {
        dump = GameObject.Find("DumpingGround");
        if (dump) {
            target = a.GameController.SnapToGround(dump.transform.position);
        } else {
            Vector2 offset = 10.0f * UnityEngine.Random.insideUnitCircle;
            target = a.GameController
                .SnapToGround(actor.transform.position + new Vector3(offset.x, 0, offset.y));
        }
    }

    public override void DoStep() {
        try {
            if (actor.MoveTowards(target, 1.1f)) {
                actor.DropCarriedResource();
                this.completed = true;
            }
        }
        catch (Exception e) {
            Debug.Log("Exception in DumpCarriedResourceOrder");
            Debug.Log(e);
            this.failed = true;
        }
    }
}
