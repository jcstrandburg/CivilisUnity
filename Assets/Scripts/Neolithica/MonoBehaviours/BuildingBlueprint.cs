using UnityEngine;

namespace Neolithica.MonoBehaviours {
    public class BuildingBlueprint : MonoBehaviour {

        // Use this for initialization
        public void Start () {
            constructMe = null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="prefab"></param>
        /// <todo>This needs to get updated to not spawn an actual building until placement or something</todo>
        public void Activate(GameObject prefab) {
            //this is probably not the best way to handle this, deal with this later
            GameObject go = GameController.Instance.Factory.Instantiate(prefab);

            constructMe = go.GetComponent<ConstructionManager>();
            constructMe.StartPlacement();
            go.transform.SetParent(transform);
            constructMe.transform.localPosition = Vector3.zero;
        }

        public void Deactivate() {
            constructMe = null;
        }


        // Update is called once per frame
        public void Update() {
            if (constructMe == null) {
                return;
            }

            if (Input.GetMouseButtonDown(1)) {
                Destroy(constructMe.gameObject);
                constructMe = null;
                Deactivate();
                return;
            }

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (!Physics.Raycast(Camera.main.transform.position, ray.direction, out hit, Mathf.Infinity,1 << LayerMask.NameToLayer("Terrain")))
                return;

            transform.position = hit.point;
            constructMe.transform.localPosition = Vector3.zero;

            if (constructMe.IsBuildable(transform.position)) {
                constructMe.GhostGood();

                if (Input.GetMouseButtonDown(0)) {
                    constructMe.transform.SetParent(GameController.Instance.transform);
                    constructMe.StartConstruction();
                    Deactivate();
                }
            } else {
                constructMe.GhostBad();
            }
        }

        private ConstructionManager constructMe;
    }
}
