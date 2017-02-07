using Neolithica.MonoBehaviours.Reservations;
using UnityEngine;

namespace Neolithica {
    public interface IResourceStore {
        float GetCapacity(string resourceTag);
        float GetAvailableContents(string resourceTag);
        void ReserveContents(GameObject reserver, string resourceTag, float amount);
        void WithdrawContents(Reservation res);
        float GetAvailableStorage(string resourceTag);
        void ReserveStorage(GameObject reserver, string resourceTag, float amount);
        void DepositContents(Reservation res);
    }
}
