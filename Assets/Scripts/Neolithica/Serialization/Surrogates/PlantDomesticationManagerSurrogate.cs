using AqlaSerializer;
using Neolithica.MonoBehaviours;
using Tofu.Serialization;

namespace Neolithica.Serialization.Surrogates {
    [SerializableType, SurrogateFor(typeof(PlantDomesticationManager))]
    public class PlantDomesticationManagerSurrogate {

        [SerializableMember(1)] public MonoBehaviourResolver Resolver { get; set; }
        [SerializableMember(2)] public double ForestGardenThreshold { get; set; }
        [SerializableMember(3)] public GameController GameController { get; set; }
        [SerializableMember(4)] public StatManager Stats { get; set; }

        public static implicit operator PlantDomesticationManagerSurrogate(PlantDomesticationManager value) {
            if (value == null)
                return null;

            return new PlantDomesticationManagerSurrogate {
                Resolver = MonoBehaviourResolver.Make(value),
                ForestGardenThreshold = value.forestGardenThreshold,
                GameController = value.GameController,
                Stats = value.Stats,
            };
        }

        public static implicit operator PlantDomesticationManager(PlantDomesticationManagerSurrogate surrogate) {
            if (surrogate == null)
                return null;

            PlantDomesticationManager x = surrogate.Resolver.Resolve<PlantDomesticationManager>();
            x.forestGardenThreshold = surrogate.ForestGardenThreshold;
            x.GameController = surrogate.GameController;
            x.Stats = surrogate.Stats;

            return x;
        }
    }
}
