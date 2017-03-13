using Neolithica.MonoBehaviours;
using Neolithica.Orders.Super;
using UnityEngine;

namespace Neolithica.Test.Mono {
    public class ReservoirHarvestStatTest : MonoBehaviour {

        public ActorController testActor;
        public NeolithicObject testReservoir;
        public float vegetablesHarvested;

        [Inject]
        public StatManager stats;
        [Inject]
        public GameController gameController;

        public void Start () {
            GameController.Instance.InjectAllObjects();
            var order = new HarvestFromReservoirOrder(testActor, testReservoir);
            testActor.OverrideOrder(order);
        }

        public void FixedUpdate() {
            vegetablesHarvested = (float)stats.Stat("vegetables-harvested").Value;
        }
    }
}
