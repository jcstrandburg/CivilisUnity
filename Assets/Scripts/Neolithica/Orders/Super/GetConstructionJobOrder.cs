using AqlaSerializer;
using Assets;
using Neolithica.MonoBehaviours;
using Neolithica.Orders.Simple;
using UnityEngine;

namespace Neolithica.Orders.Super {
    [SerializableType]
    public class GetConstructionJobOrder : BaseOrder {
        [SerializableMember(1)] private readonly Vector3 center;
        [SerializableMember(2)] private Vector3 targetPosition;

        public GetConstructionJobOrder(IOrderable a, ConstructionManager manager) {
            center = a.Transform.position;
            targetPosition = center;
        }

        public override void DoStep(IOrderable orderable) {
            if (orderable.MoveTowards(targetPosition)) {
                const float r = 5.0f;
                targetPosition = center + new Vector3(Random.Range(-r, r), 0, Random.Range(-r, r));
                targetPosition = orderable.GameController.SnapToGround(targetPosition);
            }
        }
    }
}