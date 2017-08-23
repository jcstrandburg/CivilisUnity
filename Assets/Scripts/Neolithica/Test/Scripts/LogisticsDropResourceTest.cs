using Neolithica.MonoBehaviours;
using Neolithica.MonoBehaviours.Logistics;
using Neolithica.Orders.Super;
using UnityEngine;

namespace Neolithica.Test.Scripts {
    public class LogisticsDropResourceTest : MonoBehaviour {

        public ActorController worker;
        public Resource resource;

        // Use this for initialization
        void Start() {
            var order = new StoreCarriedResourceOrder(worker);
            worker.EnqueueOrder(order);
            Invoke("CheckLogisticsNode", 0.5f);
        }

        public void CheckLogisticsNode() {
            var node = resource.GetComponent<LogisticsNode>();
            if (!node) {
                IntegrationTest.Fail("No logistics node present");
            } else if (node.LogisticsNetwork == null) {
                IntegrationTest.Fail("Node is not a member of a network");
            } else if (node.LogisticsNetwork.LogisticsManager == null) {
                IntegrationTest.Fail("Network has no logistics manager");
            }

            //Debug.Log(node.logisticsNetwork);
        }
    }
}
