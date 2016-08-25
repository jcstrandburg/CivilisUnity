using UnityEngine;
using System.Collections;

public class DebugMenu : MonoBehaviour {

    public bool test = false;

    private GameController _gameController;
    public GameController gameController {
        get {
            if (_gameController == null) {
                _gameController = GameController.Instance;
            }
            return _gameController;
        }
        set { _gameController = value; }
    }

    // Use this for initialization
    void Start () {	
	}
	
	// Update is called once per frame
	void Update () {	
	}

    public void QuickSave() {
        gameController.QuickSave();
    }

    public void QuickLoad() {
        gameController.QuickLoad();
    }

    public void AddWorker() {
        GameObject prefab = Resources.Load("Units/Worker") as GameObject;
        GameObject newWorker = gameController.factory.Instantiate(prefab);
        newWorker.transform.position = Camera.main.transform.position;
    }

    public void GameSpeed() {
        Time.timeScale *= 2;
        if (Time.timeScale > 4.0f) {
            Time.timeScale = 0.25f;
        }
        Debug.Log("Timescale: " + Time.timeScale);
    }

    public void PauseGame() {
        gameController.PauseGame();
    }

    public void ShowSaveGames() {
        string[] games = gameController.GetComponent<SaverLoader>().GetSaveGames();
        Debug.Log("Found " + games.Length + " save games");
        foreach (string s in games) {
            Debug.Log(s);
        }
    }
}
