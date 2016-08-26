using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpamLogisticNodes : MonoBehaviour {

    LogisticsNetwork network;
    public int spawnCount=20;
    private List<GameObject> spawned = new List<GameObject>();

    void Awake() {
        GameFactory f = GameController.Instance.factory;
        for (int i = 0; i < spawnCount; ++i) {
            var obj = f.Instantiate(Resources.Load("Boulder") as GameObject);
            spawned.Add(obj);
        }
        network = GetComponent<LogisticsNetwork>();
    }

	// Use this for initialization
	void Start () {
        GameFactory f = GameController.Instance.factory;
	    foreach (var obj in spawned) {
            obj.GetComponent<Resource>().SetDown();
            f.InjectObject(obj);
            obj.transform.position = this.transform.position;
            var offset = new Vector3(Random.Range(-10, 10),
                                     0,
                                     Random.Range(-10, 10));
            obj.transform.position += offset;
            obj.GetComponent<NeolithicObject>().SnapToGround();
        }
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        //var x = network.FindComponents<Warehouse>();
        //Debug.Log(x.Length);
	}
}
