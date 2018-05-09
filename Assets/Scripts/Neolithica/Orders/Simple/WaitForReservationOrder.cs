using Neolithica.MonoBehaviours;
using Neolithica.MonoBehaviours.Reservations;

namespace Neolithica.Orders.Simple {
    public class WaitForReservationOrder : IdleOrder {
        private readonly Reservation reservation;

        public WaitForReservationOrder(ActorController actor, Reservation r) : base(actor) {
            actor.GetComponent<NeolithicObject>().statusString = "Waiting for reservation";
            reservation = r;
        }

        public override void DoStep(ActorController actor) {
            base.DoStep(actor);
            if (reservation.Ready) {
                Completed = true;
            }
        }
    }
}
