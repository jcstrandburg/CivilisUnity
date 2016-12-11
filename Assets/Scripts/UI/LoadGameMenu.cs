using UnityEngine;
using System.Collections;

public class LoadGameMenu : MonoBehaviour {

    public SaverLoader SaverLoader { get; set; }
    private MenuController menuController;

	void Start () {
	    menuController = GetComponent<MenuController>();
	}

    private void PopulateSelection() {
        string[] saves = SaverLoader.GetSaveGames();
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
            SaverLoader.LoadGame(gameName);
            menuController.PopMenu();
        }
    }
}
