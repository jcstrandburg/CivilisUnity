using System;
using AqlaSerializer;
using Neolithica.MonoBehaviours;
using UnityEngine;

namespace Neolithica.Orders.Simple {
    [SerializableType]
    public class SimpleWithdrawOrder : BaseOrder {
        public SimpleWithdrawOrder(ActorController a) : base(a) {
        }

        public override void DoStep() {
            Warehouse w = Actor.resourceReservation.source.GetComponent<Warehouse>();
            try {
                var resourceType = Actor.resourceReservation.resourceKind;
                w.WithdrawReservation(Actor.resourceReservation);            
                Resource r = Actor.GameController.CreateResourcePile(resourceType, 1);
                Actor.PickupResource(r);
                this.Completed = true;
            }
            catch (Exception e) {
                Debug.Log("SimpleWithdrawOrder failed to withdraw with exception");
                Debug.Log(e);
                this.Failed = true;
            }
        }
    }
}
