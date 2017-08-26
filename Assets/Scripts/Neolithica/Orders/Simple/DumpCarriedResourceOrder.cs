using System;
using AqlaSerializer;
using Neolithica.MonoBehaviours;
using UnityEngine;

namespace Neolithica.Orders.Simple {
    /// <summary>
    /// Order to dump the currently carried resource on the ground
    /// </summary>
    [SerializableType]
    public class DumpCarriedResourceOrder : BaseOrder {
        [SerializableMember(1)]
        private GameObject dump;
        [SerializableMember(2)]
        private Vector3 target;

        public DumpCarriedResourceOrder(ActorController actor) : base() {
            dump = GameObject.Find("DumpingGround");
            if (dump) {
                target = actor.GameController.SnapToGround(dump.transform.position);
            } else {
                Vector2 offset = 10.0f * UnityEngine.Random.insideUnitCircle;
                target = actor.GameController
                    .SnapToGround(actor.transform.position + new Vector3(offset.x, 0, offset.y));
            }
        }

        public override void DoStep(ActorController actor) {
            try {
                if (actor.MoveTowards(target)) {
                    actor.DropCarriedResource();
                    Completed = true;
                }
            }
            catch (Exception e) {
                Debug.Log("Exception in DumpCarriedResourceOrder");
                Debug.Log(e);
                Failed = true;
            }
        }
    }
}
