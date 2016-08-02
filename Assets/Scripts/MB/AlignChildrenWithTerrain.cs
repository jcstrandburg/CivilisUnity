using UnityEngine;
using System.Collections;

public class AlignChildrenWithTerrain : MonoBehaviour {

    private Vector3 origin;//our current position as snapped to ground

	// Use this for initialization
	void Start () {
        origin = GameController.instance.SnapToGround(transform.position);
        foreach (Transform child in transform) {
            if (child.GetComponent<MeshRenderer>()) {
                AlignChild(child);
            }
        }
	}

    private void AlignChild(Transform child) {
        var relativeSnappedPosition = GameController.instance.SnapToGround(child.position);
        child.position += new Vector3(0.0f, relativeSnappedPosition.y - origin.y, 0.0f);
    }
}
