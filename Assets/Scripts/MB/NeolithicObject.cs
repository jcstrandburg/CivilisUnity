using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(NeolithicObject))]
public class NeolithicObjectEditor : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        NeolithicObject nobject = (NeolithicObject)target;
        if (GUILayout.Button("SnapToGround")) {
            nobject.SnapToGround(true);
        }
    }
}
#endif

public class NeolithicObject : MonoBehaviour {

	public enum Selectability {Unselectable, Selectable, Multiselectable};

    public bool snapToGround = true;
	public bool selectable = true;
	public Selectability selectability = Selectability.Selectable;
	public bool selected = false;
	public bool pointerHover = false;
	public bool orderable = false;
	public string statusString;

    //[DontSaveField]
    public ActionProfile actionProfile;

    private SelectHalo halo;

    private GameController _gameController;
    public GameController gameController {
        get {
            if (_gameController == null) {
                _gameController = GameController.Instance;
            }
            return _gameController;
        }
        set { _gameController = value; }
    }

    public void SnapToGround(bool force=false) {
        if (snapToGround || force) {
            transform.position = gameController.SnapToGround(transform.position);
        }
    }

	public virtual void Start() {
        SnapToGround();
        halo = GetComponentInChildren<SelectHalo>();
        if (actionProfile == null) {
            //Debug.Log(name);
        }
	}

    public void OnDeserialize() {
        Deselect();
    }

	public void SelectClick() {
        if (selectability != Selectability.Unselectable) {
            if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift)) {
                gameController.DeselectAll();
            }
            Select();
        }
	}

	public void ContextClick() {
        gameController.DoContextMenu(this);
	}

	public void HoverStart() { pointerHover = true; }
	public void HoverEnd() { pointerHover = false; }

	public virtual void Select() {
		selected = true;
		BroadcastMessage("OnSelect", null, SendMessageOptions.DontRequireReceiver);
        gameController.AddSelected(this);
	}
	
	public virtual void Deselect() {
		selected = false;
		BroadcastMessage("OnDeselect", null, SendMessageOptions.DontRequireReceiver);
	}

	public void Update() {
        if (halo) {
            halo.highlighted = selected || pointerHover;
        }
	}
	
	public virtual string[] GetPrimativeSelectionDialog() {
		return new string[] {"Dihydrogen monoxide"};
	}
	
	public virtual string[] UpdatePrimativeSelectionDialog() {
		return new string[] {"No selection menu"};
	}
}
