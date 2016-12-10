using UnityEngine;
using System.Collections;

public class ForceInjectTestObject : MonoBehaviour {

    void Awake() {
        GameController.Instance.factory.InjectGameobject(gameObject);
        Destroy(this);
    }
}
