using UnityEngine;

namespace Neolithica.MonoBehaviours.Reservations {
    public class ResourceReservation : Reservation {
        public GameObject source;
        public Resource.Type type;
        public double amount;
    }
}
