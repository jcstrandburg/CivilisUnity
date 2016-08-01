using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// Order to dump the currently carried resource on the ground
/// </summary>
public class DumpCarriedResourceOrder : BaseOrder {
    GameObject dump;

    public DumpCarriedResourceOrder(ActorController a) : base(a) {
        dump = GameObject.Find("DumpingGround");
        dump.GetComponent<NeolithicObject>().SnapToGround();
    }

    public override void DoStep() {
        try {
            if (actor.MoveTowards(dump.transform.position, 1.1f)) {
                actor.DropCarriedResource();
                this.completed = true;
            }
        }
        catch (Exception e) {
            Debug.Log("Exception in TempDumpOrder");
            Debug.Log(e);
            this.failed = true;
        }
    }
}
