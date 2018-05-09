using Neolithica.MonoBehaviours;
using Neolithica.Orders.Super;
using UnityEngine;
using UnityEngine.Assertions;

namespace Neolithica.Test.Mono {
    public class PlantDomesticationTestRunner : MonoBehaviour {

        public ActorController testActor;
        public GameObject testReservoir;

        [Inject]
        public StatManager stats;
        [Inject]
        public GameController gameController;

        public float vegetablesHarvested;
        public bool forestGardenForbidden;

        public void Start () {
            GameController.Instance.InjectAllObjects();
            var order = new HarvestFromReservoirOrder(testActor, testReservoir);
            testActor.OverrideOrder(order);

            Assert.IsTrue(gameController.ForbiddenActions.Contains(CommandType.ForestGarden));
        }

        public void FixedUpdate() {
            vegetablesHarvested = (float)stats.Stat("vegetables-harvested").Value;
            forestGardenForbidden = gameController.ForbiddenActions.Contains(CommandType.ForestGarden);        
        }
    }
}
