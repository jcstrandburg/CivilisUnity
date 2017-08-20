using AqlaSerializer;
using Neolithica.MonoBehaviours;
using Tofu.Serialization;

namespace Neolithica.Serialization.Surrogates {
    [SerializableType, SurrogateFor(typeof(FishingHole))]
    public class FishingHoleSurrogate {

        [SerializableMember(1)] public MonoBehaviourResolver Resolver { get; set; }
        [SerializableMember(2)] public float FishRange { get; set; }
        [SerializableMember(3)] public float FishMoveSpeed { get; set; }
        [SerializableMember(4)] public float FishTurnSpeed { get; set; }

        public static implicit operator FishingHoleSurrogate(FishingHole value) {
            if (value == null)
                return null;

            return new FishingHoleSurrogate {
                Resolver = MonoBehaviourResolver.Make(value),
                FishRange = value.fishRange,
                FishMoveSpeed = value.fishMoveSpeed,
                FishTurnSpeed = value.fishTurnSpeed,
            };
        }

        public static implicit operator FishingHole(FishingHoleSurrogate surrogate) {
            if (surrogate == null)
                return null;

            FishingHole x = surrogate.Resolver.Resolve<FishingHole>();
            x.fishRange = surrogate.FishRange;
            x.fishMoveSpeed = surrogate.FishMoveSpeed;
            x.fishTurnSpeed = surrogate.FishTurnSpeed;

            return x;
        }
    }
}
