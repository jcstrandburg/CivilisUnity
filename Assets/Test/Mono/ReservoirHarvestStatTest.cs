using UnityEngine;
using System.Collections;
using UnityEngine.Assertions;

public class ReservoirHarvestStatTest : MonoBehaviour {

    public ActorController testActor;
    public NeolithicObject testReservoir;
    public float vegetablesHarvested;

    [Inject]
    public StatManager stats;
    [Inject]
    public GameController gameController;

	public void Start () {
        GameController.Instance.InitializeAllObjects();
        var order = new HarvestFromReservoirOrder(testActor, testReservoir);
        testActor.OverrideOrder(order);
    }

    public void FixedUpdate() {
        vegetablesHarvested = (float)stats.Stat("vegetables-harvested").Value;
    }
}
