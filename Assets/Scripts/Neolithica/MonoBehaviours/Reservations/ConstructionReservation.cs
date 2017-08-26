using Tofu.Serialization;

namespace Neolithica.MonoBehaviours.Reservations {
    [SavableMonobehaviour(18)]
    public class ConstructionReservation : Reservation {
        public ResourceKind resourceKind;
        public double amount;
    }
}
