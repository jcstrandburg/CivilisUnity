using UnityEngine;
using System.Collections;
using System;

public class SimpleWithdrawOrder : BaseOrder {
    public SimpleWithdrawOrder(ActorController a) : base(a) {
    }

    public override void DoStep() {
        Warehouse w = actor.resourceReservation.source.GetComponent<Warehouse>();
        try {
            string tag = actor.resourceReservation.resourceTag;
            w.WithdrawReservation(actor.resourceReservation);            
            Resource r = actor.gameController.CreateResourcePile(tag, 1);
            actor.PickupResource(r);
            this.completed = true;
        }
        catch (Exception e) {
            Debug.Log("SimpleWithdrawOrder failed to withdraw with exception");
            Debug.Log(e);
            this.failed = true;
        }
    }
}
