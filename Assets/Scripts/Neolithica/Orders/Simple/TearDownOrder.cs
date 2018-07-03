using AqlaSerializer;
using Assets;
using Neolithica.MonoBehaviours;
using UnityEngine;

namespace Neolithica.Orders.Simple {
    /// <summary>
    /// Order to tear down the base camp (or any other building)
    /// </summary>
    [SerializableType]
    public class TearDownOrder : BaseOrder {
        [SerializableMember(1)] private readonly GameObject target;

        public TearDownOrder(IOrderable a, GameObject target) {
            a.GetComponent<NeolithicObject>().statusString = $"Tearing down {target.name}";
            this.target = target;
        }

        public override void DoStep(IOrderable orderable) {
            if (orderable.MoveTowards(target.transform.position)) {
                target.gameObject.SendMessage(nameof(IOnTearDown.OnTearDown));
                Object.Destroy(target.gameObject);
                Completed = true;
            }
        }
    }
}
