using UnityEngine;
using System.Collections;

public class PlantDomesticationManager : MonoBehaviour {

    private bool forestGardensEnabled = false;
    public bool ForestGardensEnabled {
        get {
            return forestGardensEnabled;
        }
    }

    [Inject]
    private GameController _gameController;
    public GameController gameController {
        get {
            if (_gameController == null) {
                _gameController = GameController.Instance;
            }
            return _gameController;
        }
        set { _gameController = value; }
    }

    [Inject]
    public StatManager stats;

    public double forestGardenThreshold;

    public void Start() {
        var stat = stats.Stat("vegetables-harvested");
        if (stat != null) {
            stat.OnChange += this.OnVegetablesHarvestedChange;
        }
    }

    /// <summary>
    /// Event handler for when the vegetables harvested stat changes
    /// </summary>
    /// <param name="s"></param>
    public void OnVegetablesHarvestedChange(GameStat s) {
        if ((double)s.Value >= forestGardenThreshold) {
            EnableForestGardens();
        }
    }

    public void EnableForestGardens() {
        forestGardensEnabled = true;
        if (gameController != null) {
            gameController.forbiddenActions.Remove("ForestGarden");
        }
    }
}
