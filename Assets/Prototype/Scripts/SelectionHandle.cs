using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class SelectionHandle : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler {
	public void OnPointerEnter(PointerEventData eventData) {
		transform.parent.BroadcastMessage("HoverStart");
	}

	public void OnPointerExit(PointerEventData eventData) {
		transform.parent.BroadcastMessage("HoverEnd");
	}

	public void OnPointerDown(PointerEventData eventData) {
		switch (eventData.button) {
		case PointerEventData.InputButton.Left:
			transform.parent.BroadcastMessage("SelectClick");
			break;
		case PointerEventData.InputButton.Right:
			transform.parent.BroadcastMessage("ContextClick");
			break;
		}
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
