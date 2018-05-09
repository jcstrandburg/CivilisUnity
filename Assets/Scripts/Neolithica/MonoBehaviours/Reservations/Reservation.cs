using Tofu.Serialization;
using UnityEngine;

namespace Neolithica.MonoBehaviours.Reservations {
    [SavableMonobehaviour(20)]
    public class Reservation : MonoBehaviour {

        [SerializeField] private bool ready; //if the reserved asset is ready
        [SerializeField] private bool acknowledged; //i don't know what this is for
        [SerializeField] private bool released; //if the reservation has been released by the customer
        [SerializeField] private bool cancelled; //if the reservation has been cancelled by the vendor

        public bool Ready {
            get { return ready; }
            set { ready = value; }
        }

        public bool Acknowledged {
            get { return acknowledged; }
            set { acknowledged = value; }
        }

        public bool Released {
            get { return released; }
            set { 
                released = value;
                if (!released) return;

                if (Application.isPlaying) {
                    Destroy(this);
                }
                else {
                    DestroyImmediate(this);
                }
            }
        }

        public bool Cancelled {
            get { return cancelled; }
            set { cancelled = value; }
        }
    }
}

