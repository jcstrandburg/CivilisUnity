using System.Collections.Generic;
using Neolithica.MonoBehaviours;
using UnityEngine;

namespace Neolithica.Test.Scripts {
    public class SpamLogisticNodes : MonoBehaviour {

        //LogisticsNetwork network;
        public int spawnCount=20;
        private List<GameObject> spawned = new List<GameObject>();

        [Inject]
        public GameController GameController { get; set; }

        public void Awake() {
            var factory = GameController.Factory;
            var prefab = Resources.Load("Boulder") as GameObject;
            for (var i = 0; i < spawnCount; ++i) {
                var obj = factory.Instantiate(prefab);
                spawned.Add(obj);
            }
        }

        // Use this for initialization
        public void Start () {
            var factory = GameController.Factory;
            foreach (var obj in spawned) {
                obj.GetComponent<Resource>().SetDown();
                factory.InjectGameObject(obj);
                obj.transform.position = this.transform.position;
                var offset = Random.insideUnitCircle*10;
                obj.transform.position += new Vector3(offset.x, 0, offset.y);
                obj.GetComponent<NeolithicObject>().SnapToGround();
            }
        }
	
        // Update is called once per frame
        public void FixedUpdate () {
            //var x = network.FindComponents<Warehouse>();
            //Debug.Log(x.Length);
        }
    }
}
