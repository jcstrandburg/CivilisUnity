using UnityEngine;
using System.Collections;

public class CameraController2d : MonoBehaviour {

	public float scrollSpeed;
	public float minScrollRatio;
	public float screenMargin;
	public float dragScrollSpeed;

	Camera cam;

	// Use this for initialization
	void Start () {
		cam = gameObject.GetComponent<Camera> ();
	}

	void Scroll(float x, float y) {
		if ( x != 0.0f || y != 0.0f ) {
			transform.position = new Vector3 (transform.position.x + x * cam.orthographicSize * scrollSpeed, 
			                                  transform.position.y + y * cam.orthographicSize * scrollSpeed, 
			                                  transform.position.z);
		}
	}
	
	// Update is called once per frame
	void Update () {
	
		float x = Input.GetAxis ("Mouse ScrollWheel");
		if ( x != 0.0f ) {
			cam.orthographicSize *= (1.0f-x);
		}

		Vector3 mousePos = Input.mousePosition;

		if ( Input.GetMouseButton(2)) {
			float xMov = Input.GetAxis ("Mouse X");
			float yMov = Input.GetAxis ("Mouse Y");
			transform.position += new Vector3(-xMov*cam.orthographicSize*dragScrollSpeed, 
			                                  -yMov*cam.orthographicSize*dragScrollSpeed, 
			                                  0.0f);
		}

		if ( mousePos.x >= 0.0f && mousePos.y >= 0.0 && mousePos.x < Screen.width && mousePos.y < Screen.width) {

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
			Scroll(xScroll*Time.deltaTime, yScroll*Time.deltaTime);
		}
	}
}