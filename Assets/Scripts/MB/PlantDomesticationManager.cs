﻿using UnityEngine;

public class PlantDomesticationManager : MonoBehaviour {

    private bool forestGardensEnabled = false;
    public bool ForestGardensEnabled {
        get {
            return forestGardensEnabled;
        }
    }

    [Inject]
    public GameController GameController { get; set; }
    [Inject]
    public StatManager Stats;

    public double forestGardenThreshold;

    public void Start() {
        var stat = Stats.Stat("vegetables-harvested");
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
        if (GameController != null) {
            GameController.ForbiddenActions.Remove(CommandType.ForestGarden);
        }
    }
}
