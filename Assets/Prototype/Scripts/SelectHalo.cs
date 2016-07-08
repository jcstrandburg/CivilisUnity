using UnityEngine;
using System.Collections;

public class SelectHalo : MonoBehaviour {

	private SpriteRenderer haloRenderer;

	public bool highlighted {
		get { return haloRenderer.enabled;	}
		set { haloRenderer.enabled = value;	}
	}

	void Awake() {
		haloRenderer = GetComponent<SpriteRenderer>();
		haloRenderer.enabled = false;
	}

	void OnSelect() {
		haloRenderer.enabled = true;
	}

	void OnDeselect() {
		haloRenderer.enabled = false;
	}

    void Update() {
        if (highlighted) {
            //transform.forward = GameController.instance.GetGroundNormal(transform.position); //this code seems to be borked, leave it alone for now
        }
    }
}
