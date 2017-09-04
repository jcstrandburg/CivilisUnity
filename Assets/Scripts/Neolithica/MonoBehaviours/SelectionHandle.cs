using UnityEngine;
using UnityEngine.EventSystems;

namespace Neolithica.MonoBehaviours {
    public interface IHoverable {
        void HoverStart();
        void HoverEnd();
    }

    public class SelectionHandle : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler {
        public void OnPointerEnter(PointerEventData eventData) {
            transform.parent.BroadcastMessage(nameof(IHoverable.HoverStart), SendMessageOptions.DontRequireReceiver);
        }

        public void OnPointerExit(PointerEventData eventData) {
            transform.parent.BroadcastMessage(nameof(IHoverable.HoverEnd), SendMessageOptions.DontRequireReceiver);
        }

        public void OnPointerDown(PointerEventData eventData) {
            switch (eventData.button) {
                case PointerEventData.InputButton.Left:
                    transform.parent.SendMessage(nameof(IClickable.SelectClick), SendMessageOptions.DontRequireReceiver);
                    break;
                case PointerEventData.InputButton.Right:
                    transform.parent.SendMessage(nameof(IClickable.ContextClick), SendMessageOptions.DontRequireReceiver);
                    break;
            }
        }
    }
}
