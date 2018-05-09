using System;
using UnityEngine;
using UnityEngine.UI;

namespace Neolithica.MonoBehaviours {
    public class SelectionMenuController : MonoBehaviour {
        public NeolithicObject selected;
        public Text agentName;
        public Text agentStatus;

        [Obsolete]
        public void ShowPrimative(NeolithicObject source) {
            gameObject.SetActive(true);
            selected = source;
            agentName.text = selected.name;
            agentStatus.text = selected.statusString;
        }

        public void ShowPrimative(Interactible source) {
            gameObject.SetActive(true);
            agentName.text = selected.name;
            agentStatus.text = selected.statusString;
        }

        public void Hide() {
            gameObject.SetActive(false);
        }

        // TODO: Use Interactible?
        public void FixedUpdate() {
            agentStatus.text = selected != null ? selected.statusString : "";
        }
    }
}
