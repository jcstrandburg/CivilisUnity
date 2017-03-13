using AqlaSerializer;
using Neolithica.MonoBehaviours;
using Neolithica.MonoBehaviours.Reservations;

namespace Neolithica.Orders.Simple {
    [SerializableType]
    public class CompleteConstructionReservation : BaseOrder {
        [SerializableMember(1)] private ConstructionManager manager;

        public CompleteConstructionReservation(ActorController actor, ConstructionManager manager) : base(actor)
        {
            this.manager = manager;
        }

        public override void DoStep() {
            if (Actor.MoveTowards(manager.transform.position)) {
                ConstructionReservation res = Actor.GetComponent<ConstructionReservation>();
                UnityEngine.Object.Destroy(Actor.GetCarriedResource(res.resourceResourceKind).gameObject);
                manager.FillReservation(res);
                Completed = true;
            }
        }
    }
}