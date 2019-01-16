using System.Linq;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 649
namespace Neolithica.UI {
    public class SelectList : MonoBehaviour {
        private GameObject selectedItem;
        [SerializeField]
        private GameObject content;
        [SerializeField]
        private GameObject listItemPrefab;
        public Color selectedColor;

        public string SelectedItem => selectedItem != null ? selectedItem.GetComponent<Text>().text : null;

        public void SelectItem(SelectListItem item) {
            //Debug.Log("Selecting item");
            if (selectedItem != null) {
                selectedItem.GetComponent<Text>().color = Color.black;
                selectedItem = null;
            }
            selectedItem = item.gameObject;
            selectedItem.GetComponent<Text>().color = selectedColor;
            SendMessageUpwards("OnSelectListChanged", this, SendMessageOptions.DontRequireReceiver);
        }

        public void AddItem(string text) {
            GameObject newItem = Instantiate(listItemPrefab);
            newItem.transform.SetParent(content.transform);
            newItem.GetComponent<Text>().text = text;
            newItem.GetComponent<Button>().onClick.AddListener(()=> {
                SelectItem(newItem.GetComponent<SelectListItem>());
            });
        }

        public void AddRandomItem() {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            System.Random random = new System.Random();        
            string label = new string(Enumerable.Repeat(chars, 10).Select(s => s[random.Next(s.Length)]).ToArray());
            AddItem(label);
        }
    }
}
