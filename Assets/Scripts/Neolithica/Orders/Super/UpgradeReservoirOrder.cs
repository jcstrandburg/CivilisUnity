using AqlaSerializer;
using Neolithica.MonoBehaviours;
using Neolithica.Orders.Simple;
using UnityEngine;

namespace Neolithica.Orders.Super {
    /// <summary>
    /// Order to upgrade a reservoir to it's next stage
    /// </summary>
    [SerializableType]
    public class UpgradeReservoirOrder : StatefulSuperOrder {
        [SerializableMember(1)] private readonly GameObject targetObj;
        [SerializableMember(2)] private readonly GameObject myPrefab;

        public UpgradeReservoirOrder(ActorController actor, GameObject target, GameObject prefab) {
            targetObj = target;
            myPrefab = prefab;
            GoToState(cSeekTarget, actor);
        }

        protected override void CreateStates() {
            CreateState(cSeekTarget,
                actor => new SimpleMoveOrder(actor, targetObj.transform.position),
                actor => GoToState(cDoUpgrade, actor),
                null);
            CreateState(cDoUpgrade,
                actor => new DoBuildingUpgrade(targetObj, myPrefab),
                actor => Completed = true,
                null);
        }

        private const string cSeekTarget = "seekTarget";
        private const string cDoUpgrade = "doUpgrade";
    }
}