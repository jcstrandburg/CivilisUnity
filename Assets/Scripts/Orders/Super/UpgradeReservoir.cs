using UnityEngine;
using System.Collections;

public class DoBuildingUpgrade : BaseOrder {
    NeolithicObject myTargetObj;
    GameObject myPrefab;

    public DoBuildingUpgrade(ActorController a, NeolithicObject target, GameObject prefab) : base(a) {
        myTargetObj = target;
        myPrefab = prefab;
    }

    public override void DoStep() {
        var replacement = GameController.Instance.factory.Instantiate(myPrefab);
        replacement.transform.position = myTargetObj.transform.position;
        replacement.transform.rotation = myTargetObj.transform.rotation;
        GameObject.Destroy(myTargetObj.gameObject);
        GameController.Instance.StatManager.Stat("forest-gardens").Add(1);

        completed = true;
    }
}

/// <summary>
/// Order to upgrade a reservoir to it's next stage
/// </summary>
public class UpgradeReservoirOrder : StatefulSuperOrder {
    NeolithicObject targetObj;
    GameObject myPrefab;

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
