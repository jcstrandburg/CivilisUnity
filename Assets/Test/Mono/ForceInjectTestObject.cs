using UnityEngine;
using System.Collections;

public class ForceInjectTestObject : MonoBehaviour {

    void Awake() {
        GameController.Instance.factory.InjectObject(gameObject);
        Destroy(this);
    }
}
