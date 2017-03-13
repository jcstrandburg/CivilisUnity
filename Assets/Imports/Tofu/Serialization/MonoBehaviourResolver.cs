using System.Linq;
using AqlaSerializer;
using UnityEngine;

namespace Tofu.Serialization {
    [SerializableType]
    public class MonoBehaviourResolver {

        [SerializableMember(1)]
        private GameObject gameObject;

        public static MonoBehaviourResolver Make<T>(T monoBehaviour) where T : MonoBehaviour {
            return new MonoBehaviourResolver { gameObject = monoBehaviour.gameObject };
        }

        public T Resolve<T>() where T : MonoBehaviour {
            return gameObject.GetComponents<T>().SingleOrDefault() ?? gameObject.AddComponent<T>();
        }
    }
}
