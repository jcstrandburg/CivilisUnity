using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Neolithica {
    public class SelectionHandle2D : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler {

        public void Awake() {
            if (transform.parent == null) {
                throw new InvalidOperationException($"A GameObject with {nameof(SelectionHandle2D)} component requires a parent GameObject");
            }
        }

        public void OnPointerEnter(PointerEventData eventData) {
            ExecuteEvents.ExecuteHierarchy<IHoverHandler>(gameObject, null, (x,y) => x.HoverStart());
        }

        public void OnPointerExit(PointerEventData eventData) {
            ExecuteEvents.ExecuteHierarchy<IHoverHandler>(gameObject, null, (x,y) => x.HoverEnd());
        }

        public void OnPointerDown(PointerEventData eventData) {
            // ReSharper disable once SwitchStatementMissingSomeCases(We don't care about middle clicks)
            switch (eventData.button) {
            case PointerEventData.InputButton.Left:
                if (transform.parent != null)
                    ExecuteEvents.ExecuteHierarchy<IGameClickHandler>(transform.parent.gameObject, null, (x, y) => x.OnSelectClick());
                break;
            case PointerEventData.InputButton.Right:
                if (transform.parent != null)
                    ExecuteEvents.ExecuteHierarchy<IGameClickHandler>(transform.parent.gameObject, null, (x, y) => x.OnContextClick());
                break;
            }
        }
    }
}
