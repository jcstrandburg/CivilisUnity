using AqlaSerializer;
using Neolithica.MonoBehaviours.Logistics;
using Tofu.Serialization;

namespace Neolithica.Serialization.Surrogates {
    [SerializableType, SurrogateFor(typeof(LogisticsManager))]
    public class LogisticsManagerSurrogate {

        [SerializableMember(1)] public MonoBehaviourResolver Resolver { get; set; }

        public static implicit operator LogisticsManagerSurrogate(LogisticsManager value) {
            if (value == null)
                return null;

            return new LogisticsManagerSurrogate {
                Resolver = MonoBehaviourResolver.Make(value),
            };
        }

        public static implicit operator LogisticsManager(LogisticsManagerSurrogate surrogate) {
            if (surrogate == null)
                return null;

            LogisticsManager x = surrogate.Resolver.Resolve<LogisticsManager>();

            return x;
        }
    }
}
