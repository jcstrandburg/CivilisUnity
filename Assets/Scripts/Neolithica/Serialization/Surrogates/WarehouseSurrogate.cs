using AqlaSerializer;
using Neolithica.MonoBehaviours;
using Neolithica.MonoBehaviours.Reservations;
using System.Collections.Generic;
using Tofu.Serialization;

namespace Neolithica.Serialization.Surrogates {
    [SerializableType, SurrogateFor(typeof(Warehouse))]
    public class WarehouseSurrogate {

        [SerializableMember(1)] public MonoBehaviourResolver Resolver { get; set; }
        [SerializableMember(2)] public double TotalCapacity { get; set; }
        [SerializableMember(3)] public GameController GameController { get; set; }
        [SerializableMember(4)] public List<ResourceReservation> ResourceReservations { get; set; }
        [SerializableMember(5)] public List<StorageReservation> StorageReservations { get; set; }
        [SerializableMember(6)] public ResourceProfile[] ResourceLimits { get; set; }
        [SerializableMember(7)] public ResourceProfile[] ResourceContents { get; set; }

        public static implicit operator WarehouseSurrogate(Warehouse value) {
            if (value == null)
                return null;

            return new WarehouseSurrogate {
                Resolver = MonoBehaviourResolver.Make(value),
                TotalCapacity = value.totalCapacity,
                GameController = value.GameController,
                ResourceReservations = value.resourceReservations,
                StorageReservations = value.storageReservations,
                ResourceLimits = value.resourceLimits,
                ResourceContents = value.resourceContents,
            };
        }

        public static implicit operator Warehouse(WarehouseSurrogate surrogate) {
            if (surrogate == null)
                return null;

            Warehouse x = surrogate.Resolver.Resolve<Warehouse>();
            x.totalCapacity = surrogate.TotalCapacity;
            x.GameController = surrogate.GameController;
            x.resourceReservations = surrogate.ResourceReservations;
            x.storageReservations = surrogate.StorageReservations;
            x.resourceLimits = surrogate.ResourceLimits;
            x.resourceContents = surrogate.ResourceContents;

            return x;
        }
    }
}
