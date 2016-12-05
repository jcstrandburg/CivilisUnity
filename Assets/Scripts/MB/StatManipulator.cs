using UnityEngine;

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
            var stat = stats.Stat(statName);
            stat.Add((decimal)amount);
        }
    }

    // Handles the OnDestroy event
    public void OnDestroy() {
        if (triggerType == TriggerType.Destroy || triggerType == TriggerType.CreateDestroy) {
            var stat = stats.Stat(statName);
            stat.Add(-(decimal)amount);
        }
    }
}
