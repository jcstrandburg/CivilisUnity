using Tofu.Serialization;

namespace Neolithica.MonoBehaviours.Reservations {
    [SavableMonobehaviour(18)]
    public class ConstructionReservation : Reservation {
        public ResourceKind resourceResourceKind;
        public double amount;
    }
}
