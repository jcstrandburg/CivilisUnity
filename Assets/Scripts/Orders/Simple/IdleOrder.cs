using UnityEngine;
using System.Collections;

public class IdleOrder : BaseOrder {
    private Vector3 center, targetPosition;

    public IdleOrder(ActorController a) : base(a) {
        a.GetComponent<NeolithicObject>().statusString = "Idling";
        center = targetPosition = a.transform.position;
    }

    public override void DoStep() {
        Vector3 diff = targetPosition - actor.transform.position;
        if (diff.magnitude <= actor.moveSpeed) {
            float r = 5.0f;
            targetPosition = center + new Vector3(UnityEngine.Random.Range(-r, r), 0, UnityEngine.Random.Range(-r, r));
            targetPosition = GameController.instance.SnapToGround(targetPosition);
            diff = targetPosition - actor.transform.position;
        }
        actor.transform.position += diff * 0.08f * (actor.moveSpeed / diff.magnitude);
    }
}
