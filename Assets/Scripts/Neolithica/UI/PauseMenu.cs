using UnityEngine;
using UnityEngine.SceneManagement;

namespace Neolithica.UI {
    public class PauseMenu : MonoBehaviour {

        public void QuitGame() {
            SceneManager.LoadScene("MainMenu");
        }
    }
}
