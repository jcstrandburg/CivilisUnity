using Neolithica.MonoBehaviours;
using UnityEngine;

namespace Neolithica.Orders.Simple {
    public class IdleOrder : BaseOrder {
        private readonly Vector3 center;
        private Vector3 targetPosition;

        private const float range = 5.0f;

        public IdleOrder(ActorController actor) {
            actor.GetComponent<NeolithicObject>().statusString = "Idling";
            center = targetPosition = actor.transform.position;
        }

        public override void DoStep(ActorController actor) {
            Vector3 diff = targetPosition - actor.transform.position;
            if (diff.magnitude <= actor.moveSpeed) {
                targetPosition = center + new Vector3(Random.Range(-range, range), 0, Random.Range(-range, range));
                targetPosition = actor.GameController.SnapToGround(targetPosition);
                diff = targetPosition - actor.transform.position;
            }
            actor.transform.position += diff * 0.08f * (actor.moveSpeed / diff.magnitude);
        }
    }
}
