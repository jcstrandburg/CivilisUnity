using UnityEngine;
using System.Collections;

public class LogisticsActor : MonoBehaviour {

    [Inject]
    public LogisticsManager logisticsManager {
        set; get;
    }
}
