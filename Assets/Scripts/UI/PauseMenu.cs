using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour {

	public void QuitGame() {
        SceneManager.LoadScene("MainMenu");
    }
}
