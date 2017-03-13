using AqlaSerializer;
using Neolithica.MonoBehaviours;
using Tofu.Serialization;
using UnityEngine;

namespace Neolithica.Orders.Simple {
    [SerializableType, SurrogateFor(typeof(IdleOrder))]
    public class IdleOrderSurrogate {
        [SerializableMember(1)]
        public ActorController Actor { get; set; }
        [SerializableMember(2)]
        public bool Completed { get; set; }
        [SerializableMember(3)]
        public bool Cancelled { get; set; }
        [SerializableMember(4)]
        public bool Failed { get; set; }
        [SerializableMember(5)]
        public bool Initialized { get; set; }
        [SerializableMember(6)]
        public Vector3 Center { get; set; }
    }

    public class IdleOrder : BaseOrder {
        private Vector3 center;
        private Vector3 targetPosition;

        public IdleOrder(ActorController a) : base(a) {
            a.GetComponent<NeolithicObject>().statusString = "Idling";
            center = targetPosition = a.transform.position;
        }

        public override void DoStep() {
            Vector3 diff = targetPosition - Actor.transform.position;
            if (diff.magnitude <= Actor.moveSpeed) {
                float r = 5.0f;
                targetPosition = center + new Vector3(UnityEngine.Random.Range(-r, r), 0, UnityEngine.Random.Range(-r, r));
                targetPosition = Actor.GameController.SnapToGround(targetPosition);
                diff = targetPosition - Actor.transform.position;
            }
            Actor.transform.position += diff * 0.08f * (Actor.moveSpeed / diff.magnitude);
        }

        public static implicit operator IdleOrderSurrogate(IdleOrder order) {
            if (order == null)
                return null;

            var surrogate = new IdleOrderSurrogate {
                Actor = order.Actor,
                Completed = order.Completed,
                Cancelled = order.Cancelled,
                Failed = order.Failed,
                Initialized = order.Initialized,
                Center = order.center
            };

            return surrogate;
        }

        public static implicit operator IdleOrder(IdleOrderSurrogate surrogate) {
            if (surrogate == null)
                return null;

            var order = new IdleOrder(surrogate.Actor);
            order.Completed = surrogate.Completed;
            order.Cancelled = surrogate.Cancelled;
            order.Failed = surrogate.Failed;
            order.Initialized = surrogate.Initialized;
            order.center = surrogate.Center;

            return order;
        }
    }
}
