using Neolithica.Serialization;
using UnityEngine;
using UnityEngine.UI;

namespace Neolithica.UI {
    public class SaveGameMenu : MonoBehaviour {
        public InputField saveNameField;

        [Inject]
        public SaverLoader SaverLoader { get; set; }

        void Start() {
            SaverLoader saverLoader = SaverLoader;
            string[] saves = saverLoader.GetSaveGames();
            SelectList list = GetComponentInChildren<SelectList>();
            if (list) {
                foreach (var s in saves) {
                    list.AddItem(s);
                }
            }
        }

        public void OnSelectListChanged(SelectList list) {
            var gameName = list.SelectedItem;
            saveNameField.text = gameName;
        }

        public void SaveGame() {
            var gameName = saveNameField.text;
            if (gameName != null) {
                Debug.Log("Saving game: " + gameName);
                SaverLoader.SaveGame(gameName);
                GetComponent<MenuController>().PopMenu();
            } else {
                Debug.Log("NULL");
                Debug.Log(saveNameField);
                Debug.Log(saveNameField.text);
            }
        }
    }
}
