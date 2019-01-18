using System.Linq;
using Neolithica.Extensions;
using Neolithica.Orders.Simple;
using Neolithica.ScriptableObjects;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Neolithica {
    public class Interactible : MonoBehaviour, IHoverHandler, IGameClickHandler {

        public bool IsSelectable => selectability != Selectability.Unselectable;
        public Selectability Selectability => selectability;

        public ActionProfile actionProfile;
        [SerializeField] private Selectability selectability = Selectability.Unselectable;

        private SelectionHalo2D Halo => this.CacheComponent(ref halo, GetComponentInChildren<SelectionHalo2D>);
        private SelectionHalo2D halo;

        private bool isHovered;
        private bool isSelected;

        public void HoverStart() => isHovered = true;
        public void HoverEnd() => isHovered = false;
        public void Select() => isSelected = true;
        public void Deselect() => isSelected = false;

        public void Update() {
            if (Halo != null)
                Halo.Highlighted = isHovered || isSelected;
        }

        public void OnSelectClick() {
            if (!IsSelectable)
                return;

            ExecuteEvents.ExecuteHierarchy<IInteractibleEventHandler>(
                gameObject,
                GetEventData(),
                (x,eventData) => x.OnSelectClick((InteractibleEventData) eventData));
        }

        public void OnContextClick() {
            ExecuteEvents.ExecuteHierarchy<IInteractibleEventHandler>(
                gameObject,
                GetEventData(),
                (x,eventData) => x.OnContextClick((InteractibleEventData) eventData));
        }

        private BaseEventData GetEventData() {
            return new InteractibleEventData(EventSystem.current, this);
        }
    }
}
