using AqlaSerializer;
using Neolithica.MonoBehaviours;
using UnityEngine;

namespace Neolithica.Orders.Simple {
    [SerializableType]
    public class DoBuildingUpgrade : BaseOrder {
        [SerializableMember(1)]
        private NeolithicObject myTargetObj;
        [SerializableMember(2)]
        private GameObject myPrefab;

        public DoBuildingUpgrade(ActorController a, NeolithicObject target, GameObject prefab) : base() {
            myTargetObj = target;
            myPrefab = prefab;
        }

        public override void DoStep(ActorController actor) {
            var replacement = GameController.Instance.Factory.Instantiate(myPrefab);
            replacement.transform.position = myTargetObj.transform.position;
            replacement.transform.rotation = myTargetObj.transform.rotation;
            Object.Destroy(myTargetObj.gameObject);
            actor.GameController.StatManager.Stat("forest-gardens").Add(1);

            Completed = true;
        }
    }
}