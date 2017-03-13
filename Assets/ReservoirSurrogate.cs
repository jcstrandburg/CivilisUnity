using AqlaSerializer;
using Neolithica.MonoBehaviours;
using Neolithica.MonoBehaviours.Reservations;
using System.Collections.Generic;

namespace Tofu.Serialization.Surrogates {
    [SerializableType, SurrogateFor(typeof(Reservoir))]
    public class ReservoirSurrogate {

        [SerializableMember(1)]
        public MonoBehaviourResolver Resolver { get; set; }
        [SerializableMember(2)]
        public double Amount { get; set; }
        [SerializableMember(3)]
        public double Max { get; set; }
        [SerializableMember(4)]
        public float RegenRate { get; set; }
        [SerializableMember(5)]
        public List<Reservation> Reservations { get; set; }
        [SerializableMember(6)]
        public ResourceKind ResourceResourceKind { get; set; }
        [SerializableMember(7)]
        public string HarvestStat { get; set; }

        public static implicit operator ReservoirSurrogate(Reservoir value) {
            if (value == null)
                return null;

            return new ReservoirSurrogate {
                Resolver = MonoBehaviourResolver.Make(value),
                Amount = value.amount,
                Max = value.max,
                RegenRate = value.regenRate,
                Reservations = value.reservations,
                ResourceResourceKind = value.resourceResourceKind,
                HarvestStat = value.harvestStat,
            };
        }

        public static implicit operator Reservoir(ReservoirSurrogate surrogate) {
            if (surrogate == null)
                return null;

            Reservoir x = surrogate.Resolver.Resolve<Reservoir>();
            x.amount = surrogate.Amount;
            x.max = surrogate.Max;
            x.regenRate = surrogate.RegenRate;
            x.reservations = surrogate.Reservations;
            x.resourceResourceKind = surrogate.ResourceResourceKind;
            x.harvestStat = surrogate.HarvestStat;

            return x;
        }
    }
}
