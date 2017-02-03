using UnityEngine;

/// <summary>
/// A simple stateless move order
/// </summary>
public class SimpleMoveOrder : BaseOrder {
    public Vector3 targetPosition;
    float proximity;

    public SimpleMoveOrder(ActorController a, Vector3 position, float proximity = 0.0f) : base(a) {
        a.GetComponent<NeolithicObject>().statusString = "Moving to position";
        this.proximity = proximity;
        targetPosition = position;
    }

    public override void DoStep() {
        this.completed = actor.MoveTowards(targetPosition);
        Vector3 diff = targetPosition - actor.transform.position;
        if (diff.magnitude < proximity) {
            this.completed = true;
        }
    }
}
