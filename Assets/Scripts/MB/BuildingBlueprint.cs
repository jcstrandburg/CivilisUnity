using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class BuildingBlueprint : MonoBehaviour, IPointerDownHandler {

    public GameObject displayMe {
        get {
            return transform.Find("Sphere").gameObject;
        }
    }

    string buildingType;

	// Use this for initialization
	void Start () {
        //gameObject.SetActive(false);
        displayMe.SetActive(false);
	}

    public void Activate(string type) {
        buildingType = type;
        displayMe.SetActive(true);
        //gameObject.SetActive(true);
    }

    public void Deactivate() {
        //gameObject.SetActive(false);
        displayMe.SetActive(false);
    }


    // Update is called once per frame
    void Update() {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, ray.direction, out hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("Terrain"))) {
            transform.position = hit.point;
        }
        else {

        }
    }

    public void OnPointerDown(PointerEventData eventData) {
        switch (eventData.button) {
            case PointerEventData.InputButton.Left:
                Debug.Log("I'mma build a thing: " + buildingType);
                GameObject newBuilding = Instantiate(Resources.Load(buildingType) as GameObject);
                newBuilding.transform.position = transform.position;
                Deactivate();
                break;
            case PointerEventData.InputButton.Right:
                Deactivate();
                break;
        }
    }
}
