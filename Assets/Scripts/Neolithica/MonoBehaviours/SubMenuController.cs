using Tofu.Serialization;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Neolithica.MonoBehaviours {
    [SavableMonobehaviour(4)]
    public class SubMenuController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

        public GameObject buttonPrefab;
        public bool pointerOver = false;

        // Handles the Awake event
        void Awake() {
            ClearMenu();
        }

        // Handles the Update event
        public void Update() {
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)) {
                if (!pointerOver) {
                    Debug.Log ("Clearing submenu due to mouse event!");
                    ClearMenu();
                }
            }
        }

        // Handles the OnPointerEnter event
        public void OnPointerEnter(PointerEventData ped) {
            pointerOver = true;
        }

        // Handles the OnPointerExit event
        public void OnPointerExit(PointerEventData ped) {
            pointerOver = false;
        }

        /// <summary>
        /// Clears this menu
        /// </summary>
        public void ClearMenu() {
            foreach (Transform child in transform) {
                Destroy(child.gameObject);
            }
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Adds a new button to this menu
        /// </summary>
        /// <param name="label"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public Button AddButton(string label, UnityAction action) {
            gameObject.SetActive(true);
            GameObject newButton = GameController.Instance.Factory.Instantiate(buttonPrefab);
            newButton.transform.SetParent(transform);

            newButton.GetComponent<Button>().onClick.AddListener(action);
            GameObject text = newButton.transform.Find("Text").gameObject;
            text.GetComponent<Text>().text = label;
            return newButton.GetComponent<Button>();
        }
    }
}
