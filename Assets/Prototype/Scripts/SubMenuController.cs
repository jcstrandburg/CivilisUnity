using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SubMenuController : MonoBehaviour {

    public GameObject buttonPrefab;

    void Awake() {
        ClearMenu();
    }

    public void ClearMenu() {
        foreach (Transform child in transform) {
            Destroy(child.gameObject);
        }
        gameObject.SetActive(false);
    }

	public void AddButton(string label, UnityEngine.Events.UnityAction action) {
        gameObject.SetActive(true);
        GameObject newButton = Instantiate(buttonPrefab);
        newButton.transform.SetParent(transform);

        newButton.GetComponent<Button>().onClick.AddListener(action);
        GameObject text = newButton.transform.Find("Text").gameObject;
        text.GetComponent<Text>().text = label;
    }
}
