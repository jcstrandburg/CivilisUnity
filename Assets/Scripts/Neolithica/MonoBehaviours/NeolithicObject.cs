using Neolithica.ScriptableObjects;
using Tofu.Serialization;
using UnityEngine;

namespace Neolithica.MonoBehaviours {
    [SavableMonobehaviour(26)]
    public class NeolithicObject : MonoBehaviour, IOnComponentWasInjected, IHoverable, IClickable {

        public enum Selectability { Unselectable, Selectable, Multiselectable };

        public bool snapToGround = true;
        public bool selectable = true;
        public Selectability selectability = Selectability.Selectable;
        public bool selected = false;
        public bool pointerHover = false;
        public bool orderable = false;
        public string statusString;
        public ActionProfile actionProfile;

        private SelectHalo halo;

        [Inject]
        public GameController GameController { get; set; }

        // Handles Start event
        public virtual void Start() {
            halo = GetComponentInChildren<SelectHalo>();
        }

        // Handles OnDeserialize event
        public void OnDeserialize() {
            Deselect();
        }

        // Handles SelectClick event
        public void SelectClick() {
            if (selectability == Selectability.Unselectable)
                return;

            if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift)) {
                GameController.DeselectAll();
            }
            Select();
        }

        // Handles ContextClick event
        public void ContextClick() {
            GameController.DoContextMenu(this);
        }

        // Handles HoverStart event
        public void HoverStart() { pointerHover = true; }

        // Handles HoverEnd event
        public void HoverEnd() { pointerHover = false; }

        // Handles Select event
        public virtual void Select() {
            selected = true;
            BroadcastMessage(nameof(ISelectable.OnSelect), null, SendMessageOptions.DontRequireReceiver);
            GameController.AddSelected(this);
        }

        // Handles Deselect event
        public virtual void Deselect() {
            selected = false;
            BroadcastMessage(nameof(ISelectable.OnDeselect), null, SendMessageOptions.DontRequireReceiver);
        }


        // Handles Update event
        public void Update() {
            if (halo) {
                halo.Highlighted = selected || pointerHover;
            }
        }

        /// <summary>
        /// Snaps the object to the ground. Will only actually snap if the object's snapToGround flag is true or a the force parameter is true
        /// Precondition: GameController has been injected
        /// </summary>
        /// <param name="force">If true the snapToGround field will be ignored</param>
        public void SnapToGround(bool force=false) {
            if (snapToGround || force) {
                Vector3 snappedPos = GameController.SnapToGround(transform.position);
                transform.position = snappedPos;
            }
        }

        public void OnComponentWasInjected() {
            SnapToGround();
        }
    }
}
