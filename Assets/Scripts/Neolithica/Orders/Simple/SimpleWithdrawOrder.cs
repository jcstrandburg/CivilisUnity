﻿using System;
using AqlaSerializer;
using Neolithica.MonoBehaviours;
using UnityEngine;

namespace Neolithica.Orders.Simple {
    [SerializableType]
    public class SimpleWithdrawOrder : BaseOrder {
        public override void DoStep(ActorController actor) {
            var w = actor.resourceReservation.source.GetComponent<Warehouse>();
            try {
                ResourceKind resourceType = actor.resourceReservation.resourceKind;
                w.WithdrawReservation(actor.resourceReservation);
                Resource r = actor.GameController.CreateResourcePile(resourceType, 1);
                actor.PickupResource(r);
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
