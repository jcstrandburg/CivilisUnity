using UnityEngine;

namespace Neolithica.MonoBehaviours {
    /// <summary>
    /// Handles toggling the selection halo for selectable units
    /// </summary>
    public class SelectHalo : MonoBehaviour, ISelectable {

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

        // ReSharper disable UnusedMember.Local (magic Unity method)
        void Awake() {
            projector = GetComponent<Projector>();
            if (projector) {
                projector.enabled = false;
            }
        }

        public void OnSelect() {
            Highlighted = true;
        }

        public void OnDeselect() {
            Highlighted = false;
        }
    }
}
