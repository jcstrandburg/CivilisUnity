using UnityEngine;
using UnityEngine.EventSystems;

public class SelectionHandle : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler {
	public void OnPointerEnter(PointerEventData eventData) {
		transform.parent.BroadcastMessage("HoverStart", SendMessageOptions.DontRequireReceiver);
	}

	public void OnPointerExit(PointerEventData eventData) {
		transform.parent.BroadcastMessage("HoverEnd", SendMessageOptions.DontRequireReceiver);
	}

	public void OnPointerDown(PointerEventData eventData) {
		switch (eventData.button) {
		case PointerEventData.InputButton.Left:
			transform.parent.SendMessage("SelectClick", SendMessageOptions.DontRequireReceiver);
			break;
		case PointerEventData.InputButton.Right:
			transform.parent.SendMessage("ContextClick", SendMessageOptions.DontRequireReceiver);
			break;
		}
	}
}
