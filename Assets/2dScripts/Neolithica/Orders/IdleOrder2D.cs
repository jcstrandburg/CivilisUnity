using Neolithica.Orders.Simple;
using UnityEngine;

namespace Neolithica.Orders {
    // TODO: Delete IdleOrder and rename this
    public class IdleOrder2D : BaseOrder {
        private readonly Vector3 center;
        private Vector3 targetPosition;

        private const float range = 5.0f;

        public override string StatusString => "Idling";

        public IdleOrder2D(IOrderable actor) {
            center = targetPosition = actor.Transform.position;
        }

        public override void DoStep(IOrderable orderable) {
            if (orderable.MoveTowards(targetPosition, 0.08f)) {
                targetPosition = center + new Vector3(UnityEngine.Random.Range(-range, range), UnityEngine.Random.Range(-range, range), 0);
            }
        }
    }
}
