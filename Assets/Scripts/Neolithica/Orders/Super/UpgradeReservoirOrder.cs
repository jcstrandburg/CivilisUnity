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
        [SerializableMember(1)]
        private NeolithicObject targetObj;
        [SerializableMember(2)]
        private GameObject myPrefab;

        public UpgradeReservoirOrder(ActorController actor, NeolithicObject target, GameObject prefab) {
            targetObj = target;
            myPrefab = prefab;
            GoToState("seekTarget", actor);
        }

        protected override void CreateStates() {
            CreateState("seekTarget",
                actor => new SimpleMoveOrder(actor, targetObj.transform.position),
                actor => GoToState("doUpgrade", actor),
                null);
            CreateState("doUpgrade",
                actor => new DoBuildingUpgrade(actor, targetObj, myPrefab),
                actor => Completed = true,
                null);
        }
    }
}