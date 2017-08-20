using AqlaSerializer;
using Neolithica.MonoBehaviours;
using Neolithica.MonoBehaviours.Reservations;
using Tofu.Serialization;
using UnityEngine;

namespace Neolithica.Serialization.Surrogates {
    [SerializableType, SurrogateFor(typeof(ResourceReservation))]
    public class ResourceReservationSurrogate {

        [SerializableMember(1)] public MonoBehaviourResolver Resolver { get; set; }
        [SerializableMember(2)] public double Amount { get; set; }
        [SerializableMember(3)] public GameObject Source { get; set; }
        [SerializableMember(4)] public ResourceKind ResourceKind { get; set; }

        public static implicit operator ResourceReservationSurrogate(ResourceReservation value) {
            if (value == null)
                return null;

            return new ResourceReservationSurrogate {
                Resolver = MonoBehaviourResolver.Make(value),
                Amount = value.amount,
                Source = value.source,
                ResourceKind = value.resourceKind,
            };
        }

        public static implicit operator ResourceReservation(ResourceReservationSurrogate surrogate) {
            if (surrogate == null)
                return null;

            ResourceReservation x = surrogate.Resolver.Resolve<ResourceReservation>();
            x.amount = surrogate.Amount;
            x.source = surrogate.Source;
            x.resourceKind = surrogate.ResourceKind;

            return x;
        }
    }
}
