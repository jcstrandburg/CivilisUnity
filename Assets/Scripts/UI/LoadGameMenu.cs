using UnityEngine;
using System.Collections;

public class LoadGameMenu : MonoBehaviour {
	void Start () {
        //this is kinda silly and should be done otherwise
        SaverLoader saverLoader = GameObject.FindObjectOfType<SaverLoader>();

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
            GameController.Instance.saverLoader.LoadGame(gameName);
            GetComponent<MenuController>().PopMenu();
        }
    }
}
