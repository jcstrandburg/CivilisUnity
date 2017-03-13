using AqlaSerializer;

namespace Tofu.Serialization.Surrogates {
    [SurrogateFor(typeof(Savable))]
    [SerializableType]
    public class SavableSurrogate {
        [SerializableMember(1)]
        public MonoBehaviourResolver Resolver { get; set; }
        [SerializableMember(2)]
        public string PrefabId { get; set; }

        public static implicit operator SavableSurrogate(Savable value) {
            if (value == null)
                return null;

            return new SavableSurrogate {
                Resolver = MonoBehaviourResolver.Make(value),
                PrefabId = value.PrefabId,
            };
        }

        public static implicit operator Savable(SavableSurrogate surrogate) {
            if (surrogate == null)
                return null;

            Savable mb = surrogate.Resolver.Resolve<Savable>();
            mb.PrefabId = surrogate.PrefabId;
            mb.WasRestored = true;

            return mb;
        }
    }
}