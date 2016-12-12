using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour {

	public void QuitGame() {
        Debug.Log("What");
        SceneManager.LoadScene("MainMenu");
        Debug.Log("Whoz");
    }
}
