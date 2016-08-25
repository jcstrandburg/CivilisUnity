using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using System.Collections;

public class SubMenuController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    public GameObject buttonPrefab;
	public bool pointerOver = false;

    void Awake() {
        ClearMenu();
    }

    public void ClearMenu() {
        foreach (Transform child in transform) {
            Destroy(child.gameObject);
        }
        gameObject.SetActive(false);
    }

	public Button AddButton(string label, UnityEngine.Events.UnityAction action) {
        gameObject.SetActive(true);
        GameObject newButton = GameController.Instance.factory.Instantiate(buttonPrefab);
        newButton.transform.SetParent(transform);

        newButton.GetComponent<Button>().onClick.AddListener(action);
        GameObject text = newButton.transform.Find("Text").gameObject;
        text.GetComponent<Text>().text = label;
		return newButton.GetComponent<Button>();
    }

	public void Update() {
		if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)) {
			if (!pointerOver) {
				Debug.Log ("Clearing submenu due to mouse event!");
				ClearMenu();
			}
		}
	}

	public void OnPointerEnter(PointerEventData ped) {
		pointerOver = true;
	}

	public void OnPointerExit(PointerEventData ped) {
		pointerOver = false;
	}
}
