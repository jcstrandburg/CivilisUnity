using AqlaSerializer;
using Neolithica.MonoBehaviours;
using Tofu.Serialization;

namespace Neolithica.Serialization.Surrogates {
    [SerializableType, SurrogateFor(typeof(ConstructionManager))]
    public class ConstructionManagerSurrogate {

        [SerializableMember(1)] public MonoBehaviourResolver Resolver { get; set; }
        [SerializableMember(2)] public bool Enabled { get; set; }
        [SerializableMember(3)] public GameController GameController { get; set; }
        [SerializableMember(4)] public GroundController GroundController { get; set; }

        public static implicit operator ConstructionManagerSurrogate(ConstructionManager value) {
            if (value == null)
                return null;

            return new ConstructionManagerSurrogate {
                Resolver = MonoBehaviourResolver.Make(value),
                Enabled = value.enabled,
                GameController = value.GameController,
                GroundController = value.GroundController,
            };
        }

        public static implicit operator ConstructionManager(ConstructionManagerSurrogate surrogate) {
            if (surrogate == null)
                return null;

            ConstructionManager x = surrogate.Resolver.Resolve<ConstructionManager>();
            x.enabled = surrogate.Enabled;
            x.GameController = surrogate.GameController;
            x.GroundController = surrogate.GroundController;

            return x;
        }
    }
}
