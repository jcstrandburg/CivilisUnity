using AqlaSerializer;
using Neolithica.MonoBehaviours;
using Neolithica.MonoBehaviours.Reservations;

namespace Tofu.Serialization.Surrogates {
    [SerializableType, SurrogateFor(typeof(StorageReservation))]
    public class StorageReservationSurrogate {

        [SerializableMember(1)] public MonoBehaviourResolver Resolver { get; set; }
        [SerializableMember(2)] public double Amount { get; set; }
        [SerializableMember(3)] public ResourceKind ResourceResourceKind { get; set; }
        [SerializableMember(4)] public Warehouse Warehouse { get; set; }

        public static implicit operator StorageReservationSurrogate(StorageReservation value) {
            if (value == null)
                return null;

            return new StorageReservationSurrogate {
                Resolver = MonoBehaviourResolver.Make(value),
                Amount = value.amount,
                ResourceResourceKind = value.resourceResourceKind,
                Warehouse = value.warehouse,
            };
        }

        public static implicit operator StorageReservation(StorageReservationSurrogate surrogate) {
            if (surrogate == null)
                return null;

            StorageReservation x = surrogate.Resolver.Resolve<StorageReservation>();
            x.amount = surrogate.Amount;
            x.resourceResourceKind = surrogate.ResourceResourceKind;
            x.warehouse = surrogate.Warehouse;

            return x;
        }
    }
}
