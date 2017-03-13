using AqlaSerializer;
using Neolithica.MonoBehaviours;
using UnityEngine;

namespace Neolithica.Orders.Simple {
    /// <summary>
    /// Order to tear down the base camp (or any other building)
    /// </summary>
    [SerializableType]
    public class TearDownOrder : BaseOrder {
        [SerializableMember(1)]
        private NeolithicObject target;

        public TearDownOrder(ActorController a, NeolithicObject target) : base(a) {
            a.GetComponent<NeolithicObject>().statusString = "Tearing down "+target.name;
            this.target = target;
        }

        public override void DoStep() {
            if (Actor.MoveTowards(target.transform.position)) {
                target.gameObject.SendMessage("OnTearDown");
                Object.Destroy(target.gameObject);
                this.Completed = true;
            }
        }
    }
}
