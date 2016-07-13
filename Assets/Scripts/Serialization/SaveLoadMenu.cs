using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;//for Type class
using System.Reflection;

using System.Linq;

/// <summary>
/// Part of the SerializeHelper package by Cherno.
/// http://forum.unity3d.com/threads/serializehelper-free-save-and-load-utility-de-serialize-all-objects-in-your-scene.338148/
/// </summary>
public class SaveLoadMenu : MonoBehaviour {

	public bool showMenu;
	public bool usePersistentDataPath = true;
	public string savePath;
    public Dictionary<string, GameObject> prefabDictionary;
    public SaveLoadContext context;

    // Use this for initialization
    void Start () {
		if(usePersistentDataPath == true) {
			savePath = Application.persistentDataPath + "/Saved Games/";
		}

		prefabDictionary = new Dictionary<string, GameObject>();
		GameObject[] prefabs = Resources.LoadAll<GameObject>("");
		foreach(GameObject loadedPrefab in prefabs) {
			if(loadedPrefab.GetComponent<ObjectIdentifier>()) {
				prefabDictionary.Add (loadedPrefab.name,loadedPrefab);
				//Debug.Log("Added GameObject to prefabDictionary: " + loadedPrefab.name);
			}
		}
	}
    	
	IEnumerator wait(float time) {
		yield return new WaitForSeconds(time);
	}

	//Use this for quicksaving
	public void SaveGame() {
		SaveGame("QuickSave");
	}

	//use this one for specifying a filename
	public void SaveGame(string saveGameName) {
		if(string.IsNullOrEmpty(saveGameName)) {
			Debug.Log ("SaveGameName is null or empty!");
			return;
		}

		SaveLoad.saveGamePath = savePath;

		//Create a new instance of SaveGame. This will hold all the data that should be saved in our scene.
		SaveGame newSaveGame = new SaveGame();
		newSaveGame.savegameName = saveGameName;

		List<GameObject> goList = new List<GameObject>();

		//Find all ObjectIdentifier components in the scene.
		//Since we can access the gameObject to which each one belongs with .gameObject, we thereby get all GameObject in the scene which should be saved!
		ObjectIdentifier[] objectsToSerialize = FindObjectsOfType(typeof(ObjectIdentifier)) as ObjectIdentifier[];
		//Go through the "raw" collection of components
		foreach (ObjectIdentifier objectIdentifier in objectsToSerialize) {
			//if the gameObject shouldn't be saved, for whatever reason (maybe it's a temporary ParticleSystem that will be destroyed anyway), ignore it
			if(objectIdentifier.dontSave == true) {
				Debug.Log("GameObject " + objectIdentifier.gameObject.name + " is set to dontSave = true, continuing loop.");
				continue;
			}

			//First, we will set the ID of the GO if it doesn't already have one.
			objectIdentifier.SetID();

			//store it in the goList temporarily, so we can first set it's ID (done above), 
			//then go through the list and call all OnSerialize methods on it, 
			//and finally go through the list again to pack the GO and add the packed data to the sceneObjects list of the new SaveGame.
			goList.Add(objectIdentifier.gameObject);
		}

		//This is a good time to call any functions on the GO that should be called before it gets serialized as part of a SaveGame. Example below.
		foreach(GameObject go in goList) {
			go.SendMessage("OnSerialize",SendMessageOptions.DontRequireReceiver);
		}

		foreach(GameObject go2 in goList) {
			//Convert the GameObject's data into a form that can be serialized (an instance of SceneObject),
			//and add it to the SaveGame instance's list of SceneObjects.
			newSaveGame.sceneObjects.Add(PackGameObject(go2));
		}

		//Call the static method that serialized our game and writes the data to a file.
		SaveLoad.Save(newSaveGame);
	}

	//Use this for quickloading
	public void LoadGame() {
		LoadGame("QuickSave");
	}

	//use this one for loading a saved gamt with a specific filename
	public void LoadGame(string saveGameName) {

		//First, we will destroy all objects in the scene which are not tagged with "DontDestroy" (such as Cameras, Managers of any type, and so on... things that should persist)
		ClearScene();

        //reset our id dictionary, reference resolvers
        context = new SaveLoadContext();

		//Call the static method that will attempt to load the specified file and deserialize it's data into a form that we can use
		SaveGame loadedGame = SaveLoad.Load(saveGameName, context);
		if(loadedGame == null) {
			Debug.Log("saveGameName " + saveGameName + "couldn't be found!");
			return;
		}

		//create a new list that will hold all the gameObjects we will create anew from the deserialized data
		List<GameObject> goList = new List<GameObject>();

		//iterate through the loaded game's sceneObjects list to access each stored objet's data and reconstruct it with all it's components
		foreach(SceneObject loadedObject in loadedGame.sceneObjects) {
			GameObject go_reconstructed = UnpackGameObject(loadedObject);
			if(go_reconstructed != null) {
				//Add the reconstructed GO to the list we created earlier.
				goList.Add(go_reconstructed);
			}
		}

		//Go through the list of reconstructed GOs and reassign any missing children
		foreach(GameObject go in goList) {
			string parentId = go.GetComponent<ObjectIdentifier>().idParent;
			if(string.IsNullOrEmpty(parentId) == false) {
				foreach(GameObject go_parent in goList) {
					if(go_parent.GetComponent<ObjectIdentifier>().id == parentId) {
                        //localscale is already where we want it after setting parent, so store it and reset it after setting parent
                        Vector3 storedScale = go.transform.localScale;
						go.transform.parent = go_parent.transform;
                        go.transform.localScale = storedScale;
					}
				}
			}
		}

        foreach (IReferenceResolver r in context.refResolvers) {
            r.Resolve();
        }

        //ResolveComponentReferences(loadedGame.sceneObjects, idDictionary);

		//This is when you might want to call any functions that should be called when a gameobject is loaded. Example below.
		foreach(GameObject go2 in goList) {
			go2.SendMessage("OnDeserialize",SendMessageOptions.DontRequireReceiver);
		}
	}

    /// <summary>
    /// Finishes loading process by reassigning references to any UnityEngine.Object types should all be instantiated now
    /// </summary>
    /// <param name="sceneObjects">List of scene objects with objectInstance set</param>
    /// <param name="idMap">Dictionary mapping id strings to ObjectIdentifier instances</param>
    public void ResolveComponentReferences(List<SceneObject> sceneObjects, Dictionary<string, ObjectIdentifier> idMap) {
        foreach (SceneObject so in sceneObjects) {
            foreach (ObjectComponent oc in so.objectComponents) {
                Component c = so.objectInstance.GetComponent(oc.componentName) as Component;
                Type cType = c.GetType();
                foreach (string fieldName in oc.references.Keys) {
                    UnityObjectReference r = oc.references[fieldName];
                    
                    if (idMap.ContainsKey(r.refID)) {
                        //Debug.Log("Found id " + r.refID + " for " + fieldName);
                        ObjectIdentifier oid = idMap[r.refID];
                        GameObject go = oid.gameObject;

                        if (r.typeName == "GameObject") {
                            FieldInfo f = cType.GetField(fieldName);
                            f.SetValue(c, go);
                        } else {
                            cType.GetField(fieldName).SetValue(c, oid.GetComponent(r.typeName));
                        }
                    } else {
                        Debug.Log("Unable to resolve reference for id " + r.refID);
                    }
                }
            }
        }
    }

	public void ClearScene() {
		//Clear the scene of all non-persistent GameObjects so we have a clean slate
		object[] obj = GameObject.FindObjectsOfType(typeof (GameObject));
		foreach (object o in obj) {
			GameObject go = (GameObject) o;

			//if the GO is tagged with DontDestroy, ignore it. (Cameras, Managers, etc. which should survive loading)
			//these kind of GO's shouldn't have an ObjectIdentifier component!
			//if(go.CompareTag("DontDestroy") || go.CompareTag("MainCamera")) {
            if (go.CompareTag("DontDestroy")) {
                //Debug.Log("Keeping GameObject in the scene: " + go.name);
				continue;
			}
			Destroy(go);
		}
	}
	
	public SceneObject PackGameObject(GameObject go) {
        //Debug.Log("Packing object " + go.name);
		
		ObjectIdentifier objectIdentifier = go.GetComponent<ObjectIdentifier>();

		//Now, we create a new instance of SceneObject, which will hold all the GO's data, including it's components.
		SceneObject sceneObject = new SceneObject();
		sceneObject.name = go.name;
		sceneObject.prefabName = objectIdentifier.prefabName;
		sceneObject.id = objectIdentifier.id;
		if(go.transform.parent != null && go.transform.parent.GetComponent<ObjectIdentifier>() == true) {
			sceneObject.idParent = go.transform.parent.GetComponent<ObjectIdentifier>().id;
		}
		else {
			sceneObject.idParent = null;
		}

        //in this case, we will only store MonoBehavior scripts that are on the GO. The Transform is stored as part of the ScenObject isntance (assigned further down below).
        //If you wish to store other component types, you have to find you own ways to do it if the "easy" way that is used for storing components doesn't work for them.
        List<Type> componentTypesToAdd = new List<Type>() {
            typeof(MonoBehaviour)
        };

        //This list will hold only the components that are actually stored (MonoBehavior scripts, in this case)
        List<object> components_filtered = new List<object>();

        //Collect all the components that are attached to the GO.
        //This includes MonoBehavior scripts, Renderers, Transform, Animator...
        //If it
        object[] components_raw = go.GetComponents<Component>() as object[];
        foreach (object component_raw in components_raw) {
            foreach (Type t in componentTypesToAdd) {
                if (component_raw.GetType().IsSubclassOf(t)) {
                    components_filtered.Add(component_raw);
                }
            }
        }

        foreach (object component_filtered in components_filtered) {
			sceneObject.objectComponents.Add(PackComponent(component_filtered));
		}

		//Assign all the GameObject's misc. values
		sceneObject.position = go.transform.position;
		sceneObject.localScale = go.transform.localScale;
		sceneObject.rotation = go.transform.rotation;
		sceneObject.active = go.activeSelf;

		return sceneObject;
	}
	
	public ObjectComponent PackComponent(object component) {
        //Debug.Log("Packing component " + component.GetType().Name);

		//This will go through all the fields of a component, check each one if it is serializable, and it it should be stored,
		//and puts it into the fields dictionary of a new instance of ObjectComponent,
		//with the field's name as key and the field's value as (object)value
		//for example, if a script has the field "float myFloat = 12.5f", then the key would be "myFloat" and the value "12.5f", tha latter stored as an object.

		ObjectComponent newObjectComponent = new ObjectComponent();
		
		const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
		Type componentType = component.GetType();
		FieldInfo[] fields = componentType.GetFields(flags);
		
		newObjectComponent.componentName = componentType.ToString();
		
		foreach(FieldInfo field in fields) {

            if (field != null) {
                object[] attributes = field.GetCustomAttributes(typeof(DontSaveField), true);
                bool stop = false;
                foreach (Attribute attribute in attributes) {
                    if (attribute.GetType() == typeof(DontSaveField)) {
                        //Debug.Log(attribute.GetType().Name.ToString());
                        stop = true;
                        break;
                    }
                }
                if (stop == true) {
                    continue;
                }

                bool customSerialize = (field.FieldType.GetCustomAttributes(typeof(CustomSerialize), true).Length > 0);
                bool forceSerialize = (field.GetCustomAttributes(typeof(ForceSerialize), true).Length > 0);
                if (field.FieldType == typeof(GameObject)) {
                    //Debug.Log(component.GetType().Name + " Trying to reference GameObject " + field.Name);
                    GameObject g;
                    ObjectIdentifier oid;
                    if ((g = field.GetValue(component) as GameObject) != null
                        && (oid = g.GetComponent<ObjectIdentifier>()) != null
                        && oid.HasID) {
                        newObjectComponent.references.Add(field.Name, new UnityObjectReference(oid.id, field.FieldType.Name));
                        //Debug.Log(component.GetType().Name + " Adding reference to " + field.Name + " of type " + field.FieldType.Name + " " + oid.id);
                    }
                    continue;
                } else if (field.FieldType.IsSubclassOf(typeof(MonoBehaviour))) {
                    //Debug.Log(component.GetType().Name + " Trying to reference " + field.FieldType.Name + " " + field.Name);
                    MonoBehaviour mb;
                    ObjectIdentifier oid;
                    if ((mb = field.GetValue(component) as MonoBehaviour) != null
                       && (oid = mb.GetComponent<ObjectIdentifier>()) != null
                       && oid.HasID) {
                        newObjectComponent.references.Add(field.Name, new UnityObjectReference(oid.id, field.FieldType.Name));
                        //Debug.Log(component.GetType().Name + " Adding reference to " + field.Name + " of type " + field.FieldType.Name + " " + oid.id);
                    }
                    continue;
                }

				if(field.FieldType.IsSerializable == false && !customSerialize && !forceSerialize) {
					//Debug.Log(field.Name + " (Type: " + field.FieldType + ") is not marked as serializable. Continue loop.");
					continue;
				}
				if(TypeSystem.IsEnumerableType(field.FieldType) == true || TypeSystem.IsCollectionType(field.FieldType) == true) {
					Type elementType = TypeSystem.GetElementType(field.FieldType);
					//Debug.Log(field.Name + " -> " + elementType);

					if(elementType.IsSerializable == false) {
						continue;
					}
				}
				
				newObjectComponent.fields.Add(field.Name, field.GetValue(component));
				//Debug.Log(field.Name + " (Type: " + field.FieldType + "): " + field.GetValue(component));
			}
		}
		return newObjectComponent;
	}

	public GameObject UnpackGameObject(SceneObject sceneObject) {
		//This is where our prefabDictionary above comes in. Each GameObject that was saved needs to be reconstucted, so we need a Prefab,
		//and we know which prefab it is because we stored the GameObject's prefab name in it's ObjectIdentifier/SceneObject script/class.
		//Theoretically, we could even reconstruct GO's that have no prefab by instatiating an empty GO and filling it with the required components... I'lll leave that to you.
		if(prefabDictionary.ContainsKey(sceneObject.prefabName) == false) {
			Debug.Log("Can't find key " + sceneObject.prefabName + " in SaveLoadMenu.prefabDictionary!");
			return null;
		}

        Debug.Log("Unpacking GameObject " + sceneObject.name);

		//instantiate the gameObject
		GameObject go = Instantiate(prefabDictionary[sceneObject.prefabName], sceneObject.position, sceneObject.rotation) as GameObject;
        sceneObject.objectInstance = go;
		
		//Reassign values
		go.name = sceneObject.name;
		go.transform.localScale = sceneObject.localScale;
		go.SetActive (sceneObject.active);
		
		if(go.GetComponent<ObjectIdentifier>() == false) {
            go.AddComponent<ObjectIdentifier>();
        }
		
		ObjectIdentifier idScript = go.GetComponent<ObjectIdentifier>();
		idScript.id = sceneObject.id;
		idScript.idParent = sceneObject.idParent;
		
		UnpackComponents(ref go, sceneObject);

		//Destroy any children that were not referenced as having a parent
		ObjectIdentifier[] childrenIds = go.GetComponentsInChildren<ObjectIdentifier>();
		foreach(ObjectIdentifier childrenIDScript in childrenIds) {
			if(childrenIDScript.transform != go.transform) {
				if(string.IsNullOrEmpty(childrenIDScript.id) == true) {
					Destroy (childrenIDScript.gameObject);
				}
			}
		}

		return go;
	}

	public void UnpackComponents(ref GameObject go, SceneObject sceneObject) {
		//Go through the stored object's component list and reassign all values in each component, and add components that are missing
		foreach(ObjectComponent obc in sceneObject.objectComponents) {
			
			if(go.GetComponent(obc.componentName) == false) {
				Type componentType = Type.GetType(obc.componentName);
				go.AddComponent(componentType);
			}
			
			object component = go.GetComponent(obc.componentName) as object;
            if (obc.componentName == "ObjectIdentifier") {
                ObjectIdentifier oid = component as ObjectIdentifier;
                //Debug.Log("Adding identifier " + oid.id + " to dictionary");
                context.oidReferences.Add(oid.id, oid);
            }
			
            //set the values for all basic deserializeable fields
			var type = component.GetType();
			foreach(KeyValuePair<string,object> p in obc.fields) {
				
				var fld = type.GetField(p.Key,
				                      BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic |
				                      BindingFlags.SetField);
				if (fld != null) {
					
					object value = p.Value;
					fld.SetValue(component, value);
				}
			}

            //create a reference resolver to be called upon once all objects have been deserialized
            foreach (string fieldName in obc.references.Keys) {
                UnityObjectReference r = obc.references[fieldName];
                FieldInfo f = type.GetField(fieldName);
                UnityObjectReferenceResolver resolver = new UnityObjectReferenceResolver(f, component, r, context);
                context.refResolvers.Add(resolver);
            }
        }
	}

    void OnGUI() {
        if (showMenu == true) {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("QuickSave")) {
                GameObject go = GameObject.Find("SaverLoader");
                var sl = go.GetComponent<SaverLoader>();
                sl.SaveGame();
                //SaveGame();
                return;
            }
            if (GUILayout.Button("QuickLoad")) {
                GameObject go = GameObject.Find("SaverLoader");
                var sl = go.GetComponent<SaverLoader>();
                sl.LoadGame();
                //LoadGame();
                return;
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
    }

}

