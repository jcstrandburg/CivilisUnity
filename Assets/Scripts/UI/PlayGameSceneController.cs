using UnityEngine;
using System.Collections;

public class PlayGameSceneController : MonoBehaviour {
    public void Awake() {
        GameController.Instance.UnpauseGame();
    }

    public void UnpauseGame() {
        GameController.Instance.UnpauseGame();
    }
}
