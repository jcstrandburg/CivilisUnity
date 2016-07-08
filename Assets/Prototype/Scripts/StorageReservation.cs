using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class StorageReservation : Reservation {
    public Warehouse warehouse;
    public string resourceTag;
    public float amount;
}
