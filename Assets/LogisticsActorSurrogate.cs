using AqlaSerializer;
using Neolithica.MonoBehaviours.Logistics;

namespace Tofu.Serialization.Surrogates {
    [SerializableType, SurrogateFor(typeof(LogisticsActor))]
    public class LogisticsActorSurrogate {

        [SerializableMember(1)] public MonoBehaviourResolver Resolver { get; set; }
        [SerializableMember(2)] public LogisticsManager LogisticsManager { get; set; }

        public static implicit operator LogisticsActorSurrogate(LogisticsActor value) {
            if (value == null)
                return null;

            return new LogisticsActorSurrogate {
                Resolver = MonoBehaviourResolver.Make(value),
                LogisticsManager = value.logisticsManager,
            };
        }

        public static implicit operator LogisticsActor(LogisticsActorSurrogate surrogate) {
            if (surrogate == null)
                return null;

            LogisticsActor x = surrogate.Resolver.Resolve<LogisticsActor>();
            x.logisticsManager = surrogate.LogisticsManager;

            return x;
        }
    }
}
