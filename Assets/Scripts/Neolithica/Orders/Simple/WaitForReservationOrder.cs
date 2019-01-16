using Neolithica.MonoBehaviours.Reservations;

namespace Neolithica.Orders.Simple {
    public class WaitForReservationOrder : IdleOrder {
        private readonly Reservation reservation;

        public override string StatusString => "Waiting for reservation";

        public WaitForReservationOrder(IOrderable actor, Reservation r) : base(actor) {
           reservation = r;
        }

        public override void DoStep(IOrderable orderable) {
            base.DoStep(orderable);
            if (reservation.Ready) {
                Completed = true;
            }
        }
    }
}
