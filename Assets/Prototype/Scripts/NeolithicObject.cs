using UnityEngine;
using System.Collections;

public class NeolithicObject : MonoBehaviour {

	public enum Selectability {Unselectable, Selectable, Multiselectable};

	public bool selectable = true;
	public Selectability selectability = Selectability.Selectable;
	public bool selected = false;
	public bool pointerHover = false;
	public bool orderable = false;
	public string statusString;
	public string[] targetActions;
	public string[] abilities;

    public void SnapToGround() {
        transform.position = GameController.instance.SnapToGround(transform.position);
    }

	public virtual void Start() {
        SnapToGround();
	}

    public void OnDeserialize() {
        Deselect();
    }

	public void SelectClick() {
		if ( !Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift)) {
            GameController.instance.DeselectAll();
		}
		Select();
	}

	public void ContextClick() {
        GameController.instance.DoContextMenu(this);
	}

	public void HoverStart() { pointerHover = true; }
	public void HoverEnd() { pointerHover = false; }

	public virtual void Select() {
		selected = true;
		BroadcastMessage("OnSelect", null, SendMessageOptions.DontRequireReceiver);
        GameController.instance.AddSelected(this);
		//Debug.Log ("Broadcast OnSelect");
		//transform.Find ("SelectionHandle").gameObject.GetComponent<SpriteRenderer>().enabled = true;
	}
	
	public virtual void Deselect() {
		selected = false;
		BroadcastMessage("OnDeselect", null, SendMessageOptions.DontRequireReceiver);
		//Debug.Log ("Broadcast OnDeselect");
		//transform.Find ("SelectionHandle").gameObject.GetComponent<SpriteRenderer>().enabled = false;
	}

	public void Update() {
		SelectHalo halo = GetComponentInChildren<SelectHalo>();
        if (halo) {
            halo.highlighted = selected || pointerHover;
        }
        SnapToGround();
	}
	
	public virtual string[] GetPrimativeSelectionDialog() {
		return new string[] {"Dihydrogen monoxide"};
	}
	
	public virtual string[] UpdatePrimativeSelectionDialog() {
		return new string[] {"No selection menu"};
	}
}
