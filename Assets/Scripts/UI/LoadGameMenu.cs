using UnityEngine;
using System.Collections;

public class LoadGameMenu : MonoBehaviour {
	void Start () {
        SaverLoader saverLoader = SaverLoader.instance;
        string[] saves = saverLoader.GetSaveGames();
        SelectList list = GetComponentInChildren<SelectList>();
        foreach (var s in saves) {
            list.AddItem(s);
        }
	}

    public void LoadSelectedGame() {
        SelectList list = GetComponentInChildren<SelectList>();
        string gameName = list.SelectedItem;
        if (gameName != null) {
            Debug.Log(gameName);
            SaverLoader.instance.LoadGame(gameName);
            GetComponent<MenuController>().PopMenu();
        }
    }
}
