using System;
using AqlaSerializer;
using Assets;
using Neolithica.MonoBehaviours;
using UnityEngine;

namespace Neolithica.Orders.Simple {
    [SerializableType]
    public class SimpleWithdrawOrder : BaseOrder {
        public override void DoStep(IOrderable orderable) {
            var w = orderable.ResourceReservation.source.GetComponent<Warehouse>();
            try {
                ResourceKind resourceType = orderable.ResourceReservation.resourceKind;
                w.WithdrawReservation(orderable.ResourceReservation);
                Resource r = orderable.GameController.CreateResourcePile(resourceType, 1);
                orderable.PickupResource(r);
                Completed = true;
            }
            catch (Exception e) {
                Debug.Log("SimpleWithdrawOrder failed to withdraw with exception");
                Debug.Log(e);
                Failed = true;
            }
        }
    }
}
