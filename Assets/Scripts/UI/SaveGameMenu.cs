using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SaveGameMenu : MonoBehaviour {
    public InputField saveNameField;

    void Start() {
        SaverLoader saverLoader = GameController.Instance.saverLoader;
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
            GameController.Instance.saverLoader.SaveGame(gameName);
            GetComponent<MenuController>().PopMenu();
        } else {
            Debug.Log("NULL");
            Debug.Log(saveNameField);
            Debug.Log(saveNameField.text);
        }
    }
}
