using UnityEngine;

namespace Tofu.Serialization {
    [SavableMonobehaviour(1)]
    public class Savable : MonoBehaviour {
        public string PrefabId;

        /// <summary>Defaults to false, should only be true when restored from a surrogate</summary>
        public bool WasRestored = false;
    }
}