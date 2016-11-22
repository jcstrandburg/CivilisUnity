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
    [Inject]
    public GameController gameController {
        get {
            if (_gameController == null) {
                _gameController = GameController.Instance;
            }
            return _gameController;
        }
        set { _gameController = value; }
    }

    // Handles Start event
    public virtual void Start() {
        SnapToGround();
        halo = GetComponentInChildren<SelectHalo>();
        if (actionProfile == null) {
            //Debug.Log(name);
        }
    }

    // Handles OnDeserialize event
    public void OnDeserialize() {
        Deselect();
    }

    // Handles SelectClick event
    public void SelectClick() {
        if (selectability != Selectability.Unselectable) {
            if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift)) {
                gameController.DeselectAll();
            }
            Select();
        }
    }

    // Handles ContextClick event
    public void ContextClick() {
        gameController.DoContextMenu(this);
    }

    // Handles HoverStart event
    public void HoverStart() { pointerHover = true; }

    // Handles HoverEnd event
    public void HoverEnd() { pointerHover = false; }

    // Handles Select event
    public virtual void Select() {
        selected = true;
        BroadcastMessage("OnSelect", null, SendMessageOptions.DontRequireReceiver);
        gameController.AddSelected(this);
    }

    // Handles Deselect event
    public virtual void Deselect() {
        selected = false;
        BroadcastMessage("OnDeselect", null, SendMessageOptions.DontRequireReceiver);
    }


    // Handles Update event
    public void Update() {
        if (halo) {
            halo.Highlighted = selected || pointerHover;
        }
    }

    /// <summary>
    /// Snaps the object to the ground. Will only actually snap if the object's snapToGround flag is true or a the force parameter is true
    /// </summary>
    /// <param name="force">If true the snapToGround field will be ignored</param>
    public void SnapToGround(bool force=false) {
        if (snapToGround || force) {
            transform.position = gameController.SnapToGround(transform.position);
        }
    }

    // Temporary selection dialog generator
	public virtual string[] GetPrimativeSelectionDialog() {
		return new string[] {"Dihydrogen monoxide"};
	}
	
    // Temporary selection dialog generator
	public virtual string[] UpdatePrimativeSelectionDialog() {
		return new string[] {"No selection menu"};
	}
}
