using UnityEngine;
using UnityEngine.UI;

namespace Neolithica.MonoBehaviours {
    public class SelectionMenuController : MonoBehaviour {
        public NeolithicObject selected;
        public Text agentName;
        public Text agentStatus;

        public void ShowPrimative(NeolithicObject source) {
            gameObject.SetActive(true);
            selected = source;
            agentName.text = selected.name;
            agentStatus.text = selected.statusString;
        }

        public void Hide() {
            gameObject.SetActive(false);
        }

        public void FixedUpdate() {
            agentStatus.text = selected.statusString;
        }
    }
}
