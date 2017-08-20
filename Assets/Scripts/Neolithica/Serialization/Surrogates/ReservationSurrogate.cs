using AqlaSerializer;
using Neolithica.MonoBehaviours.Reservations;
using Tofu.Serialization;

namespace Neolithica.Serialization.Surrogates {
    [SerializableType, SurrogateFor(typeof(Reservation))]
    public class ReservationSurrogate {

        [SerializableMember(1)] public MonoBehaviourResolver Resolver { get; set; }
        [SerializableMember(2)] public bool Ready { get; set; }
        [SerializableMember(3)] public bool Acknowledged { get; set; }
        [SerializableMember(4)] public bool Released { get; set; }
        [SerializableMember(5)] public bool Cancelled { get; set; }

        public static implicit operator ReservationSurrogate(Reservation value) {
            if (value == null)
                return null;

            return new ReservationSurrogate {
                Resolver = MonoBehaviourResolver.Make(value),
                Ready = value.Ready,
                Acknowledged = value.Acknowledged,
                Released = value.Released,
                Cancelled = value.Cancelled,
            };
        }

        public static implicit operator Reservation(ReservationSurrogate surrogate) {
            if (surrogate == null)
                return null;

            Reservation x = surrogate.Resolver.Resolve<Reservation>();
            x.Ready = surrogate.Ready;
            x.Acknowledged = surrogate.Acknowledged;
            x.Released = surrogate.Released;
            x.Cancelled = surrogate.Cancelled;

            return x;
        }
    }
}
