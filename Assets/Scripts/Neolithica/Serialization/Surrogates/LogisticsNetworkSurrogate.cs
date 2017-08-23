using AqlaSerializer;
using Neolithica.MonoBehaviours;
using Neolithica.MonoBehaviours.Logistics;
using Tofu.Serialization;

namespace Neolithica.Serialization.Surrogates {
    [SerializableType, SurrogateFor(typeof(LogisticsNetwork))]
    public class LogisticsNetworkSurrogate {

        [SerializableMember(1)] public MonoBehaviourResolver Resolver { get; set; }
        [SerializableMember(2)] public double Foodbuffer { get; set; }
        [SerializableMember(3)] public GameController GameController { get; set; }
        [SerializableMember(4)] public int Strategy { get; set; }
        [SerializableMember(5)] public LogisticsManager LogisticsManager { get; set; }

        public static implicit operator LogisticsNetworkSurrogate(LogisticsNetwork value) {
            if (value == null)
                return null;

            return new LogisticsNetworkSurrogate {
                Resolver = MonoBehaviourResolver.Make(value),
                Foodbuffer = value.Foodbuffer,
                GameController = value.GameController,
                Strategy = value.strategy,
                LogisticsManager = value.LogisticsManager,
            };
        }

        public static implicit operator LogisticsNetwork(LogisticsNetworkSurrogate surrogate) {
            if (surrogate == null)
                return null;

            LogisticsNetwork x = surrogate.Resolver.Resolve<LogisticsNetwork>();
            x.Foodbuffer = surrogate.Foodbuffer;
            x.GameController = surrogate.GameController;
            x.strategy = surrogate.Strategy;
            x.LogisticsManager = surrogate.LogisticsManager;

            return x;
        }
    }
}
