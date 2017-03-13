using AqlaSerializer;
using Neolithica.MonoBehaviours;

namespace Tofu.Serialization.Surrogates {
    [SerializableType, SurrogateFor(typeof(StatManipulator))]
    public class StatManipulatorSurrogate {

        [SerializableMember(1)] public MonoBehaviourResolver Resolver { get; set; }
        [SerializableMember(2)] public float Amount { get; set; }
        [SerializableMember(3)] public StatManager Stats { get; set; }
        [SerializableMember(4)] public string StatName { get; set; }
        [SerializableMember(5)] public StatManipulator.TriggerType TriggerType { get; set; }

        public static implicit operator StatManipulatorSurrogate(StatManipulator value) {
            if (value == null)
                return null;

            return new StatManipulatorSurrogate {
                Resolver = MonoBehaviourResolver.Make(value),
                Amount = value.amount,
                Stats = value.stats,
                StatName = value.statName,
                TriggerType = value.triggerType,
            };
        }

        public static implicit operator StatManipulator(StatManipulatorSurrogate surrogate) {
            if (surrogate == null)
                return null;

            StatManipulator x = surrogate.Resolver.Resolve<StatManipulator>();
            x.amount = surrogate.Amount;
            x.stats = surrogate.Stats;
            x.statName = surrogate.StatName;
            x.triggerType = surrogate.TriggerType;

            return x;
        }
    }
}
