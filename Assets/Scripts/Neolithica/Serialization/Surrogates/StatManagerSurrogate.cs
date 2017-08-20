using AqlaSerializer;
using Neolithica.MonoBehaviours;
using System.Collections.Generic;
using System.Linq;
using Tofu.Serialization;

namespace Neolithica.Serialization.Surrogates {
    [SerializableType, SurrogateFor(typeof(StatManager))]
    public class StatManagerSurrogate {

        [SerializableMember(1)] public MonoBehaviourResolver Resolver { get; set; }
        [SerializableMember(2)] public List<GameStat> Stats { get; set; }

        public static implicit operator StatManagerSurrogate(StatManager value) {
            if (value == null)
                return null;

            return new StatManagerSurrogate {
                Resolver = MonoBehaviourResolver.Make(value),
                Stats = value.Stats().ToList(),
            };
        }

        public static implicit operator StatManager(StatManagerSurrogate surrogate) {
            if (surrogate == null)
                return null;

            StatManager x = surrogate.Resolver.Resolve<StatManager>();
            x.SetStatsFromSaveGame(surrogate.Stats);

            return x;
        }
    }
}
