using Tofu.Serialization;

namespace Neolithica.MonoBehaviours.Reservations {
    [SavableMonobehaviour(28)]
    public class StorageReservation : Reservation {
        public Warehouse warehouse;
        public ResourceKind resourceKind;
        public double amount;
    }
}
