using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {
	public float transformSpeed;
	public float zoomSpeed;
	public float rotateSpeed;
	public float minFov;
	public float maxFov;
	public Camera cam;
	public float screenMargin;
	public float minScrollRatio;
    public bool edgeScrolling = false;

	void Start() {
	}

	// Use this for initialization
	void FixedUpdate () {
		if (Input.GetKey("a")) {
			transform.position -= transformSpeed*transform.right;
		} else if (Input.GetKey ("d")) {
			transform.position += transformSpeed*transform.right;
		}
		if (Input.GetKey ("w")) {
			transform.position += transformSpeed*transform.forward;
		} else if (Input.GetKey ("s")) {
			transform.position -= transformSpeed*transform.forward;
		}

		Vector3 mousePos = Input.mousePosition;
		if ( edgeScrolling && !GameController.instance.boxActive && mousePos.x >= 0.0f && mousePos.y >= 0.0 && mousePos.x < Screen.width && mousePos.y < Screen.width) {
			float xScroll = 0.0f, yScroll = 0.0f;
			if (mousePos.x < screenMargin && mousePos.x >= 0.0f) {
				xScroll = -1.0f + Mathf.Min(mousePos.x/screenMargin, 1.0f-minScrollRatio);
			}
			else if ( mousePos.x > Screen.width-screenMargin && mousePos.x < Screen.width) {
				xScroll = 1.0f - Mathf.Min((Screen.width-mousePos.x)/screenMargin, 1.0f-minScrollRatio);
			}
			if (mousePos.y < screenMargin && mousePos.y >= 0.0f) {
				yScroll = -1.0f + Mathf.Min(mousePos.y/screenMargin, 1.0f-minScrollRatio);
			}
			else if ( mousePos.y > Screen.height-screenMargin && mousePos.y < Screen.height) {
				yScroll = 1.0f - Mathf.Min((Screen.height-mousePos.y)/screenMargin, 1.0f-minScrollRatio);
			}
			transform.position += transformSpeed*(xScroll*transform.right + yScroll*transform.forward);
		}
	}

	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonDown (2)) {
			Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
		} else if (Input.GetMouseButtonUp(2)) {
			Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        

		//scroll to zoom
		float scrollWheel = Input.GetAxis("Mouse ScrollWheel");
		if (scrollWheel != 0.0f) {
			float fovChange = scrollWheel*zoomSpeed;
			cam.fieldOfView = Mathf.Clamp( cam.fieldOfView*(1.0f-fovChange), minFov, maxFov);
		}

		//camera rotate when middle mouse depressed
		if (Input.GetMouseButton(2)) {
			float mouseX = Input.GetAxis("Mouse X");
			transform.eulerAngles = transform.eulerAngles + new Vector3(0.0f, rotateSpeed*mouseX, 0.0f);
			
			float mouseY = Input.GetAxis("Mouse Y");
			cam.transform.eulerAngles = cam.transform.eulerAngles + new Vector3(-rotateSpeed*0.75f*mouseY, 0.0f, 0.0f);
			float x = cam.transform.eulerAngles.x;
			float diff = Mathf.Clamp(x, 25, 75)-x;
			cam.transform.eulerAngles += new Vector3(diff, 0, 0);
		}
	}
}
