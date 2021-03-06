using AqlaSerializer;
using Neolithica.MonoBehaviours;
using Neolithica.MonoBehaviours.Reservations;
using Tofu.Serialization;

namespace Neolithica.Serialization.Surrogates {
    [SerializableType, SurrogateFor(typeof(StorageReservation))]
    public class StorageReservationSurrogate {

        [SerializableMember(1)] public MonoBehaviourResolver Resolver { get; set; }
        [SerializableMember(2)] public double Amount { get; set; }
        [SerializableMember(3)] public ResourceKind ResourceKind { get; set; }
        [SerializableMember(4)] public Warehouse Warehouse { get; set; }

        public static implicit operator StorageReservationSurrogate(StorageReservation value) {
            if (value == null)
                return null;

            return new StorageReservationSurrogate {
                Resolver = MonoBehaviourResolver.Make(value),
                Amount = value.amount,
                ResourceKind = value.resourceKind,
                Warehouse = value.warehouse,
            };
        }

        public static implicit operator StorageReservation(StorageReservationSurrogate surrogate) {
            if (surrogate == null)
                return null;

            StorageReservation x = surrogate.Resolver.Resolve<StorageReservation>();
            x.amount = surrogate.Amount;
            x.resourceKind = surrogate.ResourceKind;
            x.warehouse = surrogate.Warehouse;

            return x;
        }
    }
}
