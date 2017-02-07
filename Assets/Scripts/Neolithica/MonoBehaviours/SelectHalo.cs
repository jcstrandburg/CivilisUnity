using UnityEngine;

namespace Neolithica.MonoBehaviours {
    /// <summary>
    /// Handles toggling the selection halo for selectable units
    /// </summary>
    public class SelectHalo : MonoBehaviour {

        private Projector projector;

        /// <summary>
        /// Property indicating whether the selection halo should be rendered
        /// </summary>
        public bool Highlighted {
            get {
                return projector.enabled;
            }
            set {
                if (projector.enabled != value)
                    projector.enabled = value;
            }
        }

        // Handles Awake event
        void Awake() {
            projector = GetComponent<Projector>();
            if (projector) {
                projector.enabled = false;
            }
        }

        // Handles OnSelect event
        void OnSelect() {
            Highlighted = true;
        }

        // Handles OnDeselect event
        void OnDeselect() {
            Highlighted = false;
        }
    }
}
