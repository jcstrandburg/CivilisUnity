using AqlaSerializer;
using Tofu.Serialization;
using UnityEngine;

namespace Neolithica.Test.Scripts
{
    [SerializableType]
    public class TestReferenceContainer
    {
        [SerializableMember(1)]
        public MonoBehaviour behaviourReference { get; set; }

        [SerializableMember(2)]
        public GameObject gameObjectReferece { get; set; }
    }

    [SavableMonobehaviour(999)]
    public class TestSavableBehaviour : MonoBehaviour
    {

        public string savableValue;
        public TestSavableBehaviour monobehaviourReference; //test savable monobehvaiour reference
        public GameObject gameObjectReferece; // test savable GameObject reference
        public TestReferenceContainer referenceContainer;
    }
}
