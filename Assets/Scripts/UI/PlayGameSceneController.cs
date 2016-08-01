using UnityEngine;
using System.Collections;

public class PlayGameSceneController : MonoBehaviour {
    public void Awake() {
        GameController.instance.UnpauseGame();
    }

    public void UnpauseGame() {
        GameController.instance.UnpauseGame();
    }
}
