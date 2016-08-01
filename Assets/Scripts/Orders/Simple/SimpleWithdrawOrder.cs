using UnityEngine;
using System.Collections;
using System;

public class SimpleWithdrawOrder : BaseOrder {
    public SimpleWithdrawOrder(ActorController a) : base(a) {
    }

    public override void DoStep() {
        //Debug.Log("Mercy me");
        //Debug.Log(actor.resourceReservation);
        //Debug.Log(actor.resourceReservation.source);
        //Debug.Log(actor.resourceReservation.source.GetComponent<Warehouse>());
        Warehouse w = actor.resourceReservation.source.GetComponent<Warehouse>();
        try {
            string tag = actor.resourceReservation.resourceTag;
            w.WithdrawReservation(actor.resourceReservation);            
            GameObject r = GameController.instance.CreateResourcePile(tag, 1.0f);
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
