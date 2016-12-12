using UnityEngine;
using System.Collections;

public class DoBuildingUpgrade : BaseOrder {
    private NeolithicObject myTargetObj;
    private GameObject myPrefab;

    public DoBuildingUpgrade(ActorController a, NeolithicObject target, GameObject prefab) : base(a) {
        myTargetObj = target;
        myPrefab = prefab;
    }

    public override void DoStep() {
        var replacement = GameController.Instance.Factory.Instantiate(myPrefab);
        replacement.transform.position = myTargetObj.transform.position;
        replacement.transform.rotation = myTargetObj.transform.rotation;
        Object.Destroy(myTargetObj.gameObject);
        actor.GameController.StatManager.Stat("forest-gardens").Add(1);

        completed = true;
    }
}

/// <summary>
/// Order to upgrade a reservoir to it's next stage
/// </summary>
public class UpgradeReservoirOrder : StatefulSuperOrder {
    private NeolithicObject targetObj;
    private GameObject myPrefab;

    public UpgradeReservoirOrder(ActorController a, NeolithicObject target, GameObject prefab) : base(a) {
        targetObj = target;
        myPrefab = prefab;
        GoToState("seekTarget");
    }

    protected override void CreateStates() {
        CreateState("seekTarget",
            () => new SimpleMoveOrder(actor, targetObj.transform.position),
            () => GoToState("doUpgrade"),
            null);
        CreateState("doUpgrade",
            () => new DoBuildingUpgrade(actor, targetObj, myPrefab),
            () => completed = true,
            null);
    }
}
