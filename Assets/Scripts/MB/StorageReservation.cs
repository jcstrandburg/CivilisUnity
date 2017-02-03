using System;

[Serializable]
public class StorageReservation : Reservation {
    public Warehouse warehouse;
    public Resource.Type resourceType;
    public double amount;
}
