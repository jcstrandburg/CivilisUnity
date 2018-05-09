using AqlaSerializer;
using Neolithica.MonoBehaviours;
using Neolithica.MonoBehaviours.Reservations;

namespace Neolithica.Orders.Simple {
    [SerializableType]
    public class CompleteConstructionReservation : BaseOrder {
        [SerializableMember(1)] private readonly ConstructionManager manager;

        public CompleteConstructionReservation(ConstructionManager manager)
        {
            this.manager = manager;
        }

        public override void DoStep(ActorController actor) {
            if (actor.MoveTowards(manager.transform.position)) {
                ConstructionReservation res = actor.GetComponent<ConstructionReservation>();
                UnityEngine.Object.Destroy(actor.GetCarriedResource(res.resourceKind).gameObject);
                manager.FillReservation(res);
                Completed = true;
            }
        }
    }
}