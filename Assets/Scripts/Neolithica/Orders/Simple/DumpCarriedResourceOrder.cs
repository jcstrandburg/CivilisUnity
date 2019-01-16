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
        [SerializableMember(2)] private readonly Vector3 target;

        public DumpCarriedResourceOrder(IOrderable actor) {
            GameObject dump = GameObject.Find("DumpingGround");
            if (dump) {
                target = actor.GameController.SnapToGround(dump.transform.position);
            } else {
                Vector2 offset = 10.0f * UnityEngine.Random.insideUnitCircle;
                target = actor.GameController
                    .SnapToGround(actor.Transform.position + new Vector3(offset.x, 0, offset.y));
            }
        }

        public override void DoStep(IOrderable orderable) {
            try {
                if (orderable.MoveTowards(target)) {
                    orderable.DropCarriedResource();
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
