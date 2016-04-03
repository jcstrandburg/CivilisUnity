using UnityEngine;
using System.Collections;

public class CivilisObject : MonoBehaviour {

	public bool selectable = true;
	public bool selected = false;

	public virtual void Select() {
		selected = true;
		transform.Find ("SelectionHandle").gameObject.GetComponent<SpriteRenderer>().enabled = true;
	}
	
	public virtual void Deselect() {
		selected = false;
		transform.Find ("SelectionHandle").gameObject.GetComponent<SpriteRenderer>().enabled = false;
	}

	public virtual string[] GetPrimativeSelectionDialog() {
		return new string[] {"Dihydrogen monoxide"};
	}

	public virtual string[] UpdatePrimativeSelectionDialog() {
		return new string[] {"No selection menu"};
	}
}
