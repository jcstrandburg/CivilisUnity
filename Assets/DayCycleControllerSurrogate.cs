using AqlaSerializer;
using Neolithica.MonoBehaviours;

namespace Tofu.Serialization.Surrogates {
    [SerializableType, SurrogateFor(typeof(DayCycleController))]
    public class DayCycleControllerSurrogate {

        [SerializableMember(1)] public MonoBehaviourResolver Resolver { get; set; }
        [SerializableMember(2)] public float Daytime { get; set; }

        public static implicit operator DayCycleControllerSurrogate(DayCycleController value) {
            if (value == null)
                return null;

            return new DayCycleControllerSurrogate {
                Resolver = MonoBehaviourResolver.Make(value),
                Daytime = value.daytime,
            };
        }

        public static implicit operator DayCycleController(DayCycleControllerSurrogate surrogate) {
            if (surrogate == null)
                return null;

            DayCycleController x = surrogate.Resolver.Resolve<DayCycleController>();
            x.daytime = surrogate.Daytime;

            return x;
        }
    }
}
