using AqlaSerializer;
using Neolithica.MonoBehaviours;
using Neolithica.Orders.Simple;
using UnityEngine;

namespace Neolithica.Orders.Super {
    [SerializableType]
    public class GetConstructionJobOrder : BaseOrder {
        [SerializableMember(1)]
        private ConstructionManager manager;
        [SerializableMember(2)]
        private Vector3 center;
        [SerializableMember(3)]
        private Vector3 targetPosition;

        public GetConstructionJobOrder(ActorController a, ConstructionManager manager): base() 
        {
            center = a.transform.position;
            targetPosition = center;
            this.manager = manager;
        }

        public override void DoStep(ActorController actor) {
            Vector3 diff = targetPosition - actor.transform.position;
            if (diff.magnitude <= actor.moveSpeed) {
                Debug.Log("Trying to get job reservation");
                if (manager.GetJobReservation(actor)) {
                    Debug.Log("Got job reservation");
                    Completed = true;
                    return;
                }
                float r = 5.0f;
                targetPosition = center + new Vector3(UnityEngine.Random.Range(-r, r), 0, UnityEngine.Random.Range(-r, r));
                targetPosition = actor.GameController.SnapToGround(targetPosition);
                diff = targetPosition - actor.transform.position;
            }
            actor.transform.position += diff * 0.08f * (actor.moveSpeed / diff.magnitude);
        }
    }
}