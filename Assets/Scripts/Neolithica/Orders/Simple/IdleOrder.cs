using Neolithica.MonoBehaviours;
using UnityEngine;

namespace Neolithica.Orders.Simple {
    public class IdleOrder : BaseOrder {
        private Vector3 center;
        private Vector3 targetPosition;

        public IdleOrder(ActorController actor) : base() {
            actor.GetComponent<NeolithicObject>().statusString = "Idling";
            center = targetPosition = actor.transform.position;
        }

        public override void DoStep(ActorController actor) {
            Vector3 diff = targetPosition - actor.transform.position;
            if (diff.magnitude <= actor.moveSpeed) {
                float r = 5.0f;
                targetPosition = center + new Vector3(UnityEngine.Random.Range(-r, r), 0, UnityEngine.Random.Range(-r, r));
                targetPosition = actor.GameController.SnapToGround(targetPosition);
                diff = targetPosition - actor.transform.position;
            }
            actor.transform.position += diff * 0.08f * (actor.moveSpeed / diff.magnitude);
        }
    }
}
