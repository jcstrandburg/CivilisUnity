using AqlaSerializer;
using Neolithica.MonoBehaviours;
using UnityEngine;

namespace Tofu.Serialization.Surrogates {
    [SerializableType, SurrogateFor(typeof(SubMenuController))]
    public class SubMenuControllerSurrogate {

        [SerializableMember(1)] public MonoBehaviourResolver Resolver { get; set; }
        [SerializableMember(2)] public bool PointerOver { get; set; }
        [SerializableMember(3)] public GameObject ButtonPrefab { get; set; }

        public static implicit operator SubMenuControllerSurrogate(SubMenuController value) {
            if (value == null)
                return null;

            return new SubMenuControllerSurrogate {
                Resolver = MonoBehaviourResolver.Make(value),
                PointerOver = value.pointerOver,
                ButtonPrefab = value.buttonPrefab,
            };
        }

        public static implicit operator SubMenuController(SubMenuControllerSurrogate surrogate) {
            if (surrogate == null)
                return null;

            SubMenuController x = surrogate.Resolver.Resolve<SubMenuController>();
            x.pointerOver = surrogate.PointerOver;
            x.buttonPrefab = surrogate.ButtonPrefab;

            return x;
        }
    }
}
