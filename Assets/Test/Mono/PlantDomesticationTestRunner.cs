using UnityEngine;
using System.Collections;
using UnityEngine.Assertions;

public class PlantDomesticationTestRunner : MonoBehaviour {

    public ActorController testActor;
    public NeolithicObject testReservoir;

    [Inject]
    public StatManager stats;
    [Inject]
    public GameController gameController;

    public float vegetablesHarvested;
    public bool forestGardenForbidden;

	void Start () {
        GameController.Instance.InitializeAllObjects();
        var order = new HarvestFromReservoirOrder(testActor, testReservoir);
        testActor.OverrideOrder(order);

        Assert.IsTrue(gameController.forbiddenActions.Contains("ForestGarden"));
    }

    void FixedUpdate() {
        vegetablesHarvested = (float)stats.Stat("vegetables-harvested").Value;
        forestGardenForbidden = gameController.forbiddenActions.Contains("ForestGarden");        
    }
}
