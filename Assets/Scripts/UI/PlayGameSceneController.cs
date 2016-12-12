using UnityEngine;
using System.Collections;

public class PlayGameSceneController : MonoBehaviour {
    [Inject]
    public GameController GameController { get; set; }

    public void Start() {
        GameController.UnpauseGame();
    }

    public void UnpauseGame() {
        GameController.UnpauseGame();
    }
}
