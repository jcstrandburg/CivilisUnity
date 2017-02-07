using Neolithica.MonoBehaviours;
using UnityEngine;

namespace Neolithica.UI {
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
}
