using AqlaSerializer;
using Assets;
using Neolithica.MonoBehaviours;
using UnityEngine;

namespace Neolithica.Orders.Simple {
    /// <summary>
    /// A simple stateless move order
    /// </summary>
    [SerializableType]
    public class SimpleMoveOrder : BaseOrder {
        [SerializableMember(1)] private readonly Vector3 targetPosition;
        [SerializableMember(2)] private readonly float proximity;

        public SimpleMoveOrder(IOrderable a, Vector3 position, float proximity = 0.0f) {
            a.GetComponent<NeolithicObject>().statusString = "Moving to position";
            this.proximity = proximity;
            targetPosition = position;
        }

        public override void DoStep(IOrderable orderable) {
            Completed = orderable.MoveTowards(targetPosition);
            Vector3 diff = targetPosition - orderable.Transform.position;
            if (diff.magnitude < proximity) {
                Completed = true;
            }
        }
    }
}
