using UnityEngine;

/// <summary>
/// Order to tear down the base camp (or any other building)
/// </summary>
public class TearDownOrder : BaseOrder {
    NeolithicObject target;

    public TearDownOrder(ActorController a, NeolithicObject target) : base(a) {
        a.GetComponent<NeolithicObject>().statusString = "Tearing down "+target.name;
        this.target = target;
    }

    public override void DoStep() {
        if (actor.MoveTowards(target.transform.position)) {
            target.gameObject.SendMessage("OnTearDown");
            Object.Destroy(target.gameObject);
            this.completed = true;
        }
    }
}
