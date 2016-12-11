using UnityEngine;
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

	public void Start () {
        GameController.Instance.InitializeAllObjects();
        var order = new HarvestFromReservoirOrder(testActor, testReservoir);
        testActor.OverrideOrder(order);

        Assert.IsTrue(gameController.ForbiddenActions.Contains("ForestGarden"));
	}

    public void FixedUpdate() {
        vegetablesHarvested = (float)stats.Stat("vegetables-harvested").Value;
        forestGardenForbidden = gameController.ForbiddenActions.Contains("ForestGarden");        
    }
}
