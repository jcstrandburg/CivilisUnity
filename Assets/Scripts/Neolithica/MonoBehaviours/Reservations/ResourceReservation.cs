using Tofu.Serialization;
using UnityEngine;

namespace Neolithica.MonoBehaviours.Reservations {
    [SavableMonobehaviour(10)]
    public class ResourceReservation : Reservation {
        public GameObject source;
        public ResourceKind resourceKind;
        public double amount;
    }
}
