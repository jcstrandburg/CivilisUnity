using Neolithica.MonoBehaviours;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Neolithica.UI {
    public class PauseMenu : MonoBehaviour {

        public void OnDestroy() {
            GameController.Instance.UnpauseGame();
        }

        public void QuitGame() {
            SceneManager.LoadScene("MainMenu");
        }
    }
}
