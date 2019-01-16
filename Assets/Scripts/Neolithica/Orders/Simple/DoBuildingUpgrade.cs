using AqlaSerializer;
using Neolithica.MonoBehaviours;
using UnityEngine;

namespace Neolithica.Orders.Simple {
    [SerializableType]
    public class DoBuildingUpgrade : BaseOrder {
        [SerializableMember(1)] private readonly GameObject myTargetObj;
        [SerializableMember(2)] private readonly GameObject myPrefab;

        public DoBuildingUpgrade(GameObject target, GameObject prefab) {
            myTargetObj = target;
            myPrefab = prefab;
        }

        public override void DoStep(IOrderable orderable) {
            var replacement = GameController.Instance.Factory.Instantiate(myPrefab);
            replacement.transform.position = myTargetObj.transform.position;
            replacement.transform.rotation = myTargetObj.transform.rotation;
            Object.Destroy(myTargetObj.gameObject);
            orderable.GameController.StatManager.Stat("forest-gardens").Add(1);

            Completed = true;
        }
    }
}