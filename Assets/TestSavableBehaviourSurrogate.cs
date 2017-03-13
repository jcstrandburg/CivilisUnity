using AqlaSerializer;

namespace Tofu.Serialization.Surrogates {
    [SerializableType, SurrogateFor(typeof(TestSavableBehaviour))]
    public class TestSavableBehaviourSurrogate {

        [SerializableMember(1)] public MonoBehaviourResolver Resolver { get; set; }
        [SerializableMember(2)] public bool Enabled { get; set; }
        [SerializableMember(3)] public UnityEngine.GameObject GameObjectReferece { get; set; }
        [SerializableMember(4)] public TestSavableBehaviour MonobehaviourReference { get; set; }
        [SerializableMember(5)] public TestReferenceContainer ReferenceContainer { get; set; }
        [SerializableMember(6)] public string SavableValue { get; set; }

        public static implicit operator TestSavableBehaviourSurrogate(TestSavableBehaviour value) {
            if (value == null)
                return null;

            return new TestSavableBehaviourSurrogate {
                Resolver = MonoBehaviourResolver.Make(value),
                Enabled = value.enabled,
                GameObjectReferece = value.gameObjectReferece,
                MonobehaviourReference = value.monobehaviourReference,
                ReferenceContainer = value.referenceContainer,
                SavableValue = value.savableValue,
            };
        }

        public static implicit operator TestSavableBehaviour(TestSavableBehaviourSurrogate surrogate) {
            if (surrogate == null)
                return null;

            TestSavableBehaviour x = surrogate.Resolver.Resolve<TestSavableBehaviour>();
            x.enabled = surrogate.Enabled;
            x.gameObjectReferece = surrogate.GameObjectReferece;
            x.monobehaviourReference = surrogate.MonobehaviourReference;
            x.referenceContainer = surrogate.ReferenceContainer;
            x.savableValue = surrogate.SavableValue;

            return x;
        }
    }
}
