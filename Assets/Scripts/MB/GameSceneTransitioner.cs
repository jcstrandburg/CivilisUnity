using UnityEngine;
using System.Collections;

public class GameSceneTransitioner : MonoBehaviour {

    private NewGameSettings newGameSettings;
    private string loadGameName = null;

    public void InitLoadGame(string loadGameName) {
        this.loadGameName = loadGameName;
    }

    public void InitNewGame(NewGameSettings settings) {
        newGameSettings = settings;
    }

    void Start() {
        DontDestroyOnLoad(gameObject);
    }

	void OnLevelWasLoaded() {
        Debug.Log("Level was loaded");
        if (loadGameName != null) {
            Debug.Log("I wanna load " + loadGameName);
            GameController.Instance.saverLoader.LoadGame(loadGameName);
        } else {
            Debug.Log("I don't know what to do here");
            var groundController = GameController.Instance.groundController;
            groundController.ApplySettings(newGameSettings);
            groundController.GenerateMap();
        }
        Destroy(gameObject);
    }
}
