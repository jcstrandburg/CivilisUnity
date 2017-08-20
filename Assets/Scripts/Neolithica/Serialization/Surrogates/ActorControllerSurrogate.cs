using AqlaSerializer;
using Neolithica.MonoBehaviours;
using Neolithica.Orders.Simple;
using Neolithica.ScriptableObjects;
using System.Collections.Generic;
using Tofu.Serialization;

namespace Neolithica.Serialization.Surrogates {
    [SerializableType, SurrogateFor(typeof(ActorController))]
    public class ActorControllerSurrogate {

        [SerializableMember(1)] public MonoBehaviourResolver Resolver { get; set; }
        [SerializableMember(2)] public ActionProfile ActionProfile { get; set; }
        [SerializableMember(3)] public BaseOrder CurrentOrder { get; set; }
        [SerializableMember(4)] public bool Enabled { get; set; }
        [SerializableMember(5)] public bool FollowContours { get; set; }
        [SerializableMember(6)] public GameController GameController { get; set; }
        [SerializableMember(7)] public double Health { get; set; }
        [SerializableMember(8)] public bool Idle { get; set; }
        [SerializableMember(9)] public float MoveSpeed { get; set; }
        [SerializableMember(10)] public bool Orderable { get; set; }
        [SerializableMember(11)] public List<BaseOrder> OrderQueue { get; set; }
        [SerializableMember(12)] public bool PointerHover { get; set; }
        [SerializableMember(13)] public NeolithicObject.Selectability Selectability { get; set; }
        [SerializableMember(14)] public bool Selectable { get; set; }
        [SerializableMember(15)] public bool Selected { get; set; }
        [SerializableMember(16)] public bool SnapToGround { get; set; }
        [SerializableMember(17)] public string StatusString { get; set; }

        public static implicit operator ActorControllerSurrogate(ActorController value) {
            if (value == null)
                return null;

            return new ActorControllerSurrogate {
                Resolver = MonoBehaviourResolver.Make(value),
                ActionProfile = value.actionProfile,
                CurrentOrder = value.currentOrder,
                Enabled = value.enabled,
                FollowContours = value.followContours,
                GameController = value.GameController,
                Health = value.health,
                Idle = value.idle,
                MoveSpeed = value.moveSpeed,
                Orderable = value.orderable,
                OrderQueue = value.orderQueue,
                PointerHover = value.pointerHover,
                Selectability = value.selectability,
                Selectable = value.selectable,
                Selected = value.selected,
                SnapToGround = value.snapToGround,
                StatusString = value.statusString,
            };
        }

        public static implicit operator ActorController(ActorControllerSurrogate surrogate) {
            if (surrogate == null)
                return null;

            ActorController x = surrogate.Resolver.Resolve<ActorController>();
            x.actionProfile = surrogate.ActionProfile;
            x.currentOrder = surrogate.CurrentOrder;
            x.enabled = surrogate.Enabled;
            x.followContours = surrogate.FollowContours;
            x.GameController = surrogate.GameController;
            x.health = surrogate.Health;
            x.idle = surrogate.Idle;
            x.moveSpeed = surrogate.MoveSpeed;
            x.orderable = surrogate.Orderable;
            x.orderQueue = surrogate.OrderQueue;
            x.pointerHover = surrogate.PointerHover;
            x.selectability = surrogate.Selectability;
            x.selectable = surrogate.Selectable;
            x.selected = surrogate.Selected;
            x.snapToGround = surrogate.SnapToGround;
            x.statusString = surrogate.StatusString;

            return x;
        }
    }
}
