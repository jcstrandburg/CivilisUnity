using System;
using Neolithica.MonoBehaviours;
using UnityEngine;

namespace Neolithica.Orders.Simple {
    public class SimpleWithdrawOrder : BaseOrder {
        public SimpleWithdrawOrder(ActorController a) : base(a) {
        }

        public override void DoStep() {
            Warehouse w = actor.resourceReservation.source.GetComponent<Warehouse>();
            try {
                var resourceType = actor.resourceReservation.type;
                w.WithdrawReservation(actor.resourceReservation);            
                Resource r = actor.GameController.CreateResourcePile(resourceType, 1);
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
}
