using AqlaSerializer;
using Neolithica.MonoBehaviours;
using UnityEngine;

namespace Tofu.Serialization.Surrogates {
    [SerializableType, SurrogateFor(typeof(AnimalController))]
    public class AnimalControllerSurrogate {

        [SerializableMember(1)] public MonoBehaviourResolver Resolver { get; set; }
        [SerializableMember(2)] public bool Wild { get; set; }
        [SerializableMember(3)] public float WanderRange { get; set; }
        [SerializableMember(4)] public int Age { get; set; }
        [SerializableMember(5)] public Vector3 TargetLocation { get; set; }

        public static implicit operator AnimalControllerSurrogate(AnimalController value) {
            if (value == null)
                return null;

            return new AnimalControllerSurrogate {
                Resolver = MonoBehaviourResolver.Make(value),
                Wild = value.wild,
                WanderRange = value.wanderRange,
                Age = value.age,
                TargetLocation = value.targetLocation,
            };
        }

        public static implicit operator AnimalController(AnimalControllerSurrogate surrogate) {
            if (surrogate == null)
                return null;

            AnimalController x = surrogate.Resolver.Resolve<AnimalController>();
            x.wild = surrogate.Wild;
            x.wanderRange = surrogate.WanderRange;
            x.age = surrogate.Age;
            x.targetLocation = surrogate.TargetLocation;

            return x;
        }
    }
}
