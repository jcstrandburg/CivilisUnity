using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Responds to scene change events by loading a saved game or starting a new game
/// </summary>
public class GameSceneTransitioner : MonoBehaviour {

    /// <summary>The settings to apply when a new game is loaded</summary>
    private NewGameSettings newGameSettings;
    /// <summary>Name of the save game file to load when the scene transitions</summary>
    private string loadGameName = null;

    // Handles Start event
    void Start() {
        DontDestroyOnLoad(gameObject);
    }

    // Handles OnEnable event
    void OnEnable() {
        SceneManager.sceneLoaded += DoSceneTransition;
    }

    // Handles OnDisable event
    void OnDisable() {
        SceneManager.sceneLoaded -= DoSceneTransition;
    }

    /// <summary>
    /// Initializes the transitioner to load a saved game on scene change
    /// </summary>
    /// <param name="loadGameName"></param>
    public void InitLoadGame(string loadGameName) {
        this.loadGameName = loadGameName;
    }

    /// <summary>
    /// Initializes the transitioner to create a new game on scene change
    /// </summary>
    /// <param name="settings"></param>
    public void InitNewGame(NewGameSettings settings) {
        newGameSettings = settings;
    }

    /// <summary>
    /// Does the scene transition
    /// </summary>
    /// <param name="scene"></param>
    /// <param name="mode"></param>
    void DoSceneTransition(Scene scene, LoadSceneMode mode) {
        //Debug.Log("Level was loaded");
        if (loadGameName != null) {
            //Debug.Log("I wanna load " + loadGameName);
            GameController.Instance.SaverLoader.LoadGame(loadGameName);
        }
        else {
            //Debug.Log("I don't know what to do here");
            var groundController = GameController.Instance.GroundController;
            groundController.ApplySettings(newGameSettings);
            groundController.GenerateMap();
        }
        Destroy(gameObject);
    }
}
