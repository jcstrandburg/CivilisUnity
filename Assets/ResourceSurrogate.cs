using AqlaSerializer;
using Neolithica.MonoBehaviours;

namespace Tofu.Serialization.Surrogates {
    [SerializableType, SurrogateFor(typeof(Resource))]
    public class ResourceSurrogate {

        [SerializableMember(1)] public MonoBehaviourResolver Resolver { get; set; }
        [SerializableMember(2)] public bool Preserved { get; set; }
        [SerializableMember(3)] public double Amount { get; set; }
        [SerializableMember(4)] public float Timer { get; set; }
        [SerializableMember(5)] public GameController GameController { get; set; }
        [SerializableMember(6)] public ResourceKind ResourceKind { get; set; }

        public static implicit operator ResourceSurrogate(Resource value) {
            if (value == null)
                return null;

            return new ResourceSurrogate {
                Resolver = MonoBehaviourResolver.Make(value),
                Preserved = value.preserved,
                Amount = value.amount,
                Timer = value.timer,
                GameController = value.GameController,
                ResourceKind = value.resourceKind,
            };
        }

        public static implicit operator Resource(ResourceSurrogate surrogate) {
            if (surrogate == null)
                return null;

            Resource x = surrogate.Resolver.Resolve<Resource>();
            x.preserved = surrogate.Preserved;
            x.amount = surrogate.Amount;
            x.timer = surrogate.Timer;
            x.GameController = surrogate.GameController;
            x.resourceKind = surrogate.ResourceKind;

            return x;
        }
    }
}
