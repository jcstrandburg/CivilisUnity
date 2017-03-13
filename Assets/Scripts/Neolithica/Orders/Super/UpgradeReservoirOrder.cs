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

        public UpgradeReservoirOrder(ActorController a, NeolithicObject target, GameObject prefab) : base(a) {
            targetObj = target;
            myPrefab = prefab;
            GoToState("seekTarget");
        }

        protected override void CreateStates() {
            CreateState("seekTarget",
                () => new SimpleMoveOrder(Actor, targetObj.transform.position),
                () => GoToState("doUpgrade"),
                null);
            CreateState("doUpgrade",
                () => new DoBuildingUpgrade(Actor, targetObj, myPrefab),
                () => Completed = true,
                null);
        }
    }
}