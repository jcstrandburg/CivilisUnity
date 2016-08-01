//Add an ObjectIdentifier component to each Prefab that might possibly be serialized and deserialized.
//The name variable is not used by the serialization; it is just there so you can name your prefabs any way you want, 
//while the "in-game" name can be something different
//for example, an item that the play can inspect might have the prefab name "sword_01_a", 
//but the name (not the GameObject name; that is the prefab name! We are talking about the variable "name" here!) can be "Short Sword", 
//which is what the player will see when inspecting it.
//To clarify again: A GameObject's (and thus, prefab's) name should be the same as prefabName, while the varialbe "name" in this script can be anything you want (or nothing at all).

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(ObjectIdentifier))]
public class ObjectIdentifierInspector : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        ObjectIdentifier oid = (ObjectIdentifier)target;

        if (GUILayout.Button("Find Prefab")) {
            oid.FindPrefab();
        }
    }
}
#endif

/// <summary>
/// Part of the SerializeHelper package by Cherno.
/// http://forum.unity3d.com/threads/serializehelper-free-save-and-load-utility-de-serialize-all-objects-in-your-scene.338148/
/// </summary>
public class ObjectIdentifier : MonoBehaviour {
	
	public new string name;
	public string prefabName;

	public string id;
	public string idParent;
	public bool dontSave = false;


#if UNITY_EDITOR
    public void Reset() {
        FindPrefab();
    }

    public void FindPrefab() {
        Object obj = PrefabUtility.GetPrefabParent(gameObject);
        if (obj != null) {
            string path = AssetDatabase.GetAssetPath(obj);
            string name = obj.name;
            prefabName = name;
            Debug.Log(path);
        }
    }
#endif

    public bool HasID {
        get {
            return !string.IsNullOrEmpty(id);
        }
    }

	public void SetID() {
        if (string.IsNullOrEmpty(id)) {
            id = System.Guid.NewGuid().ToString();
        }
        if (transform.parent == null) {
            idParent = null;
        }
        else {
            ObjectIdentifier oi = transform.parent.GetComponent<ObjectIdentifier>();
            if (oi) {
                oi.SetID();
                idParent = oi.id;
            } else {
                idParent = null;
            }
        }
        //CheckForRelatives();
	}
	
	private void CheckForRelatives() {
		ObjectIdentifier[] childrenIds = GetComponentsInChildren<ObjectIdentifier>();
		foreach(ObjectIdentifier idScript in childrenIds) {
			if(idScript.transform.gameObject != gameObject) {
				idScript.idParent = id;
				idScript.SetID();
			}
		}
	}
}

