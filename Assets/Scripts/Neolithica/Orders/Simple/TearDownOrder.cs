using AqlaSerializer;
using Neolithica.MonoBehaviours;
using UnityEngine;

namespace Neolithica.Orders.Simple {
    /// <summary>
    /// Order to tear down the base camp (or any other building)
    /// </summary>
    [SerializableType]
    public class TearDownOrder : BaseOrder {
        [SerializableMember(1)] private readonly GameObject target;

        public override string StatusString => $"Tearing down {target.name}";

        public TearDownOrder(IOrderable a, GameObject target) {
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
