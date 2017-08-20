using AqlaSerializer;
using Neolithica.MonoBehaviours;
using Neolithica.ScriptableObjects;
using Tofu.Serialization;

namespace Neolithica.Serialization.Surrogates {
    [SerializableType, SurrogateFor(typeof(NeolithicObject))]
    public class NeolithicObjectSurrogate {

        [SerializableMember(1)] public MonoBehaviourResolver Resolver { get; set; }
        [SerializableMember(2)] public ActionProfile ActionProfile { get; set; }
        [SerializableMember(3)] public bool SnapToGround { get; set; }
        [SerializableMember(4)] public bool Selectable { get; set; }
        [SerializableMember(5)] public bool Selected { get; set; }
        [SerializableMember(6)] public bool PointerHover { get; set; }
        [SerializableMember(7)] public bool Orderable { get; set; }
        [SerializableMember(8)] public GameController GameController { get; set; }
        [SerializableMember(9)] public NeolithicObject.Selectability Selectability { get; set; }
        [SerializableMember(10)] public string StatusString { get; set; }

        public static implicit operator NeolithicObjectSurrogate(NeolithicObject value) {
            if (value == null)
                return null;

            return new NeolithicObjectSurrogate {
                Resolver = MonoBehaviourResolver.Make(value),
                ActionProfile = value.actionProfile,
                SnapToGround = value.snapToGround,
                Selectable = value.selectable,
                Selected = value.selected,
                PointerHover = value.pointerHover,
                Orderable = value.orderable,
                GameController = value.GameController,
                Selectability = value.selectability,
                StatusString = value.statusString,
            };
        }

        public static implicit operator NeolithicObject(NeolithicObjectSurrogate surrogate) {
            if (surrogate == null)
                return null;

            NeolithicObject x = surrogate.Resolver.Resolve<NeolithicObject>();
            x.actionProfile = surrogate.ActionProfile;
            x.snapToGround = surrogate.SnapToGround;
            x.selectable = surrogate.Selectable;
            x.selected = surrogate.Selected;
            x.pointerHover = surrogate.PointerHover;
            x.orderable = surrogate.Orderable;
            x.GameController = surrogate.GameController;
            x.selectability = surrogate.Selectability;
            x.statusString = surrogate.StatusString;

            return x;
        }
    }
}
