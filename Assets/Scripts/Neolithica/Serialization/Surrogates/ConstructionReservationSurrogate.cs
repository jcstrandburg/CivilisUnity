using AqlaSerializer;
using Neolithica.MonoBehaviours;
using Neolithica.MonoBehaviours.Reservations;
using Tofu.Serialization;

namespace Neolithica.Serialization.Surrogates {
    [SerializableType, SurrogateFor(typeof(ConstructionReservation))]
    public class ConstructionReservationSurrogate {

        [SerializableMember(1)] public MonoBehaviourResolver Resolver { get; set; }
        [SerializableMember(2)] public bool Acknowledged { get; set; }
        [SerializableMember(3)] public double Amount { get; set; }
        [SerializableMember(4)] public bool Cancelled { get; set; }
        [SerializableMember(5)] public bool Enabled { get; set; }
        [SerializableMember(6)] public bool Ready { get; set; }
        [SerializableMember(7)] public bool Released { get; set; }
        [SerializableMember(8)] public ResourceKind ResourceResourceKind { get; set; }

        public static implicit operator ConstructionReservationSurrogate(ConstructionReservation value) {
            if (value == null)
                return null;

            return new ConstructionReservationSurrogate {
                Resolver = MonoBehaviourResolver.Make(value),
                Acknowledged = value.Acknowledged,
                Amount = value.amount,
                Cancelled = value.Cancelled,
                Enabled = value.enabled,
                Ready = value.Ready,
                Released = value.Released,
                ResourceResourceKind = value.resourceResourceKind,
            };
        }

        public static implicit operator ConstructionReservation(ConstructionReservationSurrogate surrogate) {
            if (surrogate == null)
                return null;

            ConstructionReservation x = surrogate.Resolver.Resolve<ConstructionReservation>();
            x.Acknowledged = surrogate.Acknowledged;
            x.amount = surrogate.Amount;
            x.Cancelled = surrogate.Cancelled;
            x.enabled = surrogate.Enabled;
            x.Ready = surrogate.Ready;
            x.Released = surrogate.Released;
            x.resourceResourceKind = surrogate.ResourceResourceKind;

            return x;
        }
    }
}
