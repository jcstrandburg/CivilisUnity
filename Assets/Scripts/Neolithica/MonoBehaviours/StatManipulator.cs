using UnityEngine;

namespace Neolithica.MonoBehaviours {
    public class StatManipulator : MonoBehaviour {

        public enum TriggerType {
            Destroy,
            Create,
            CreateDestroy,
        };

        public string statName;
        public float amount;
        public TriggerType triggerType;
        [Inject]
        public StatManager stats;

        // Handles the Start event
        public void Start() {
            if (triggerType == TriggerType.Create || triggerType == TriggerType.CreateDestroy) {
                stats.Stat(statName).Add((decimal)amount);
            }
        }

        // Handles the OnDestroy event
        public void OnDestroy() {
            if (triggerType == TriggerType.Destroy || triggerType == TriggerType.CreateDestroy) {
                stats.Stat(statName).Add(-(decimal)amount);
            }
        }
    }
}
