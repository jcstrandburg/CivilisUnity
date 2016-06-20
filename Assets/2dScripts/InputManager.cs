using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class InputManager : MonoBehaviour {

	public float minMarqueeSize = 3.0f;
	List<GameObject> selected = new List<GameObject>();

	MainMapController mapController;
	GameUIController uiController;

	bool marqueeActive = false;
	bool mouseOverUI;
	Vector3 marqueeStart;
	Vector3 marqueeEnd;

	// Use this for initialization
	void Start () {	
		mapController = GameObject.Find ("Map").GetComponent<MainMapController>();
		uiController = GameObject.Find ("GameUICanvas").GetComponent<GameUIController>();
	}

	void FixedUpdate() {
	}

	void DeselectAll() {
		Debug.Log ("Desel all");
		foreach (GameObject sel in selected) {
			sel.GetComponent<CivilisObject>().Deselect();
		}
		uiController.subMenu.ClearMenu();
		selected.Clear();
	}

	void DoPointSelect() {
		Collider2D[] colliders = Physics2D.OverlapPointAll(marqueeEnd, 1<<LayerMask.NameToLayer("SelectionHandles"));
		if (!Input.GetKey ("left shift") && !Input.GetKey ("right shift")) {
			DeselectAll();
		}

		if ( colliders.Length > 0 ) {
			foreach (Collider2D col in colliders) {
				GameObject obj = col.transform.parent.gameObject;
				CivilisObject civObj = obj.GetComponent<CivilisObject>();
				civObj.Select();
				selected.Add(obj);
				//uiController.selectionMenu.ShowPrimative(civObj);
				break;
			}
		}
	}

	void DoMarqueeSelect() {
		Collider2D[] colliders = Physics2D.OverlapAreaAll(marqueeStart, marqueeEnd, 1<<LayerMask.NameToLayer("SelectionHandles"));
		if (!Input.GetKey ("left shift") && !Input.GetKey ("right shift")) {
			DeselectAll();
		}
		
		if ( colliders.Length > 0 ) {
			foreach (Collider2D col in colliders) {
				GameObject obj = col.transform.parent.gameObject;
				CivilisObject civObj = obj.GetComponent<CivilisObject>();
				civObj.Select();
				selected.Add(obj);
			}
		}
	}

	public void SetMouseOverGUI(bool status) {
		mouseOverUI = status;
		print (status);
	}

	// Update is called once per frame
	void Update () {
		Vector3 gameMousePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -Camera.main.transform.position.z));

		//left click/selection
		if ( !mouseOverUI) {
			if (Input.GetMouseButtonDown (0)) {
				marqueeActive = true;
				marqueeStart = gameMousePos;
				marqueeEnd = gameMousePos;

				Debug.Log("Wat");
				uiController.subMenu.ClearMenu();
				uiController.HideContextMenu();
			}
			marqueeEnd = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -Camera.main.transform.position.z));
			if ( Input.GetMouseButtonUp (0)) {
				marqueeActive = false;

				uiController.selectionMenu.Hide ();

				float dist = (marqueeEnd - marqueeStart).magnitude;
				if (dist > minMarqueeSize) {
					print ("doing marquee select");
					DoMarqueeSelect();
				}
				else {
					print ("doing point select");
					DoPointSelect();
				}

				if (selected.Count == 1) {
					//uiController.selectionMenu.ShowPrimative(selected[0].GetComponent<CivilisObject>());
				}
			}

			if (Input.GetMouseButtonDown (1)) {
				uiController.HideContextMenu();

				Collider2D context = Physics2D.OverlapPoint (gameMousePos, 1<<LayerMask.NameToLayer("ContextHandles"));

				if ( context != null ) {
					print ("Got it");
					GameObject targObj = context.transform.parent.gameObject;
					print (targObj.name);

					ContextController ctext = targObj.GetComponent<ContextController>();
					print (ctext);
					//uiController.ShowContextMenu(ctext.actions);
				}
				else {
					foreach (GameObject sel in selected) {
						SimpleMoveOrder2d order = new SimpleMoveOrder2d(sel, gameMousePos);
						if ( Input.GetKey ("left shift") || Input.GetKey ("right shift")) {
							sel.GetComponent<ActorController2d>().EnqueueOrder(order);
						}
						else {
							sel.GetComponent<ActorController2d>().SetOrder(order);
						}
					}
				}
			}
		}

		if ( Input.GetKeyDown(KeyCode.F5)) {
			print ("F5 pressed");
			mapController.Randomize ();
			mapController.RefreshUVs();
		}

	}

	void OnGUI() {
		if (marqueeActive && (marqueeEnd - marqueeStart).magnitude > minMarqueeSize) {

			Vector3 start = Camera.main.WorldToScreenPoint(marqueeStart);
			Vector3 end = Camera.main.WorldToScreenPoint(marqueeEnd);

			if ( end.x < start.x) {
				float temp = end.x;
				end = new Vector3(start.x, end.y, end.z);
				start = new Vector3(temp, start.y, start.z);
			}
			if ( end.y > start.y ) {
				float temp = end.y;
				end = new Vector3(end.x, start.y, end.z);
				start = new Vector3(start.x, temp, start.z);
			}

			GUI.Box (new Rect (start.x, Screen.height-start.y, end.x-start.x, start.y-end.y), "");
		}
	}
}