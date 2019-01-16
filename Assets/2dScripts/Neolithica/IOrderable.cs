using Neolithica.MonoBehaviours;
using Neolithica.MonoBehaviours.Reservations;
using UnityEngine;

namespace Neolithica {
    public interface IOrderable {
        Transform Transform { get; }
        GameController GameController { get; }
        GameObject GameObject { get; }
        ResourceReservation ResourceReservation { get; }
        StorageReservation StorageReservation { get; }

        void DropCarriedResource();
        Resource GetCarriedResource(ResourceKind? resourceKind=null);
        T GetComponent<T>();
        bool MoveTowards(Vector3 transformPosition, float moveRatio=1.0f);
        void PickupResource(Resource res);
    }
}
