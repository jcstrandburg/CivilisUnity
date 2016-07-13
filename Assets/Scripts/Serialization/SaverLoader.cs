using UnityEngine;
using System.Collections;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

public class SaverLoader : MonoBehaviour {
    private SaveLoadContext saveLoadContext = new SaveLoadContext();
    private Dictionary<string, GameObject> prefabDictionary;
    private List<string> surrogateSerializeFields = null;
    private List<string> customSerializeFields = null;

    private Dictionary<string, GameObject> getPrefabs() {
        if (prefabDictionary == null) {
            prefabDictionary = new Dictionary<string, GameObject>();
            GameObject[] prefabs = Resources.LoadAll<GameObject>("");
            foreach (GameObject loadedPrefab in prefabs) {
                if (loadedPrefab.GetComponent<ObjectIdentifier>()) {
                    prefabDictionary.Add(loadedPrefab.name, loadedPrefab);
                }
            }
        }
        return prefabDictionary;
    }

    private SurrogateSelector getSurrogateSelector(StreamingContext context) {
        if (surrogateSerializeFields == null) {
            surrogateSerializeFields = new List<string>() {
                "Vector3", "Texture2D", "Color", "GameObject", "Transform", "Quaternion"
            };
            customSerializeFields = new List<string>();
            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes()) {
                if (type.GetCustomAttributes(typeof(CustomSerialize), true).Length > 0) {
                    customSerializeFields.Add(type.Name);
                }
            }
        }
        saveLoadContext.autoSerilizeTypes = surrogateSerializeFields.Concat(customSerializeFields).ToList();

        var s = new SurrogateSelector();
        s.AddSurrogate(typeof(Vector3), context, new Vector3Surrogate());
        s.AddSurrogate(typeof(Texture2D), context, new Texture2DSurrogate());
        s.AddSurrogate(typeof(Color), context, new ColorSurrogate());
        s.AddSurrogate(typeof(GameObject), context, new GameObjectSurrogate());
        s.AddSurrogate(typeof(Transform), context, new TransformSurrogate());
        s.AddSurrogate(typeof(Quaternion), context, new QuaternionSurrogate());

        foreach (string typeName in customSerializeFields) {
            var surrogate = new CustomSerializedObjectSurrogate();
            s.AddSurrogate(Type.GetType(typeName), context, surrogate);
        }
        return s;
    }

    public ObjectComponent PackComponent(object component) {
        ObjectComponent newObjectComponent = new ObjectComponent();

        const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
        Type componentType = component.GetType();
        FieldInfo[] fields = componentType.GetFields(flags);

        newObjectComponent.componentName = componentType.ToString();

        foreach (FieldInfo field in fields) {

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
                bool autoSerialize = (saveLoadContext.autoSerilizeTypes.Contains(field.FieldType.Name));
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
                }
                else if (field.FieldType.IsSubclassOf(typeof(MonoBehaviour))) {
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
                else if (saveLoadContext.FieldSerializeable(field)) {
                    newObjectComponent.fields.Add(field.Name, field.GetValue(component));
                } else {
                    Debug.LogFormat("{0} is not serializable", field.Name);
                }               
            }
        }
        return newObjectComponent;
    }

    public SceneObject PackGameObject(GameObject go) {
        ObjectIdentifier objectIdentifier = go.GetComponent<ObjectIdentifier>();

        //Now, we create a new instance of SceneObject, which will hold all the GO's data, including it's components.
        SceneObject sceneObject = new SceneObject();
        sceneObject.name = go.name;
        sceneObject.prefabName = objectIdentifier.prefabName;
        sceneObject.id = objectIdentifier.id;
        ObjectIdentifier parentID;
        if (go.transform.parent != null 
            && (parentID = go.transform.parent.GetComponent<ObjectIdentifier>()) != null) 
        {
            sceneObject.idParent = parentID.id;
        }
        else {
            sceneObject.idParent = null;
        }

        List<Type> componentTypesToAdd = new List<Type>() {
            typeof(MonoBehaviour)
        };
        List<object> components_filtered = new List<object>();

        object[] components_raw = go.GetComponents<Component>() as object[];
        foreach (object component_raw in components_raw) {
            foreach (Type t in componentTypesToAdd) {
                if (component_raw.GetType().IsSubclassOf(t)) {
                    ObjectComponent comp = PackComponent(component_raw);
                    sceneObject.objectComponents.Add(comp);
                }
            }
        }

        //Assign all the GameObject's misc. values
        sceneObject.position = go.transform.position;
        sceneObject.localScale = go.transform.localScale;
        sceneObject.rotation = go.transform.rotation;
        sceneObject.active = go.activeSelf;

        return sceneObject;
    }

    public SaveGame PackSaveGame(string name) {
        SaveGame newSaveGame = new SaveGame();
        newSaveGame.savegameName = name;

        List<GameObject> goList = new List<GameObject>();
        ObjectIdentifier[] objectsToSerialize = FindObjectsOfType(typeof(ObjectIdentifier)) as ObjectIdentifier[];
        foreach (var objectIdentifier in objectsToSerialize) {
            if (objectIdentifier.dontSave == true) {
                //Debug.Log("GameObject " + objectIdentifier.gameObject.name + " is set to dontSave = true, continuing loop.");
                continue;
            }

            objectIdentifier.SetID();
            goList.Add(objectIdentifier.gameObject);
        }

        //This is a good time to call any functions on the GO that should be called before it gets serialized as part of a SaveGame. Example below.
        foreach (GameObject go in goList) {
            go.SendMessage("OnSerialize", SendMessageOptions.DontRequireReceiver);
        }

        foreach (GameObject go2 in goList) {
            newSaveGame.sceneObjects.Add(PackGameObject(go2));
        }
        return newSaveGame;
    }

    private void ClearScene() {
        object[] obj = GameObject.FindObjectsOfType(typeof(GameObject));
        foreach (object o in obj) {
            GameObject go = (GameObject)o;
            if (!go.CompareTag("DontDestroy")) {
                Destroy(go);
            }
        }
    }

    private void AddResolver(IReferenceResolver res) {
        saveLoadContext.refResolvers.Add(res);
    }

    private void UnpackComponents(ref GameObject go, SceneObject sceneObject) {
        foreach (ObjectComponent obc in sceneObject.objectComponents) {
            if (go.GetComponent(obc.componentName) == false) {
                Type componentType = Type.GetType(obc.componentName);
                go.AddComponent(componentType);
            }

            object component = go.GetComponent(obc.componentName) as object;
            if (obc.componentName == "ObjectIdentifier") {
                ObjectIdentifier oid = component as ObjectIdentifier;
            }

            //set the values for all basic deserializeable fields
            var type = component.GetType();
            foreach (KeyValuePair<string, object> p in obc.fields) {

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
                UnityObjectReferenceResolver resolver = new UnityObjectReferenceResolver(f, component, r, saveLoadContext);
                AddResolver(resolver);
            }
        }
    }

    private GameObject UnpackGameObject(SceneObject sceneObject) {
        Dictionary<string, GameObject> prefabs = getPrefabs();

        if (prefabs.ContainsKey(sceneObject.prefabName) == false) {
            Debug.Log("Can't find key " + sceneObject.prefabName + " in SaveLoadMenu.prefabDictionary!");
            return null;
        }

        //Debug.Log("Unpacking GameObject " + sceneObject.name);

        //instantiate the gameObject
        GameObject go = Instantiate(prefabs[sceneObject.prefabName], sceneObject.position, sceneObject.rotation) as GameObject;
        sceneObject.objectInstance = go;

        //Reassign values
        go.name = sceneObject.name;
        go.transform.localScale = sceneObject.localScale;
        go.SetActive(sceneObject.active);

        if (go.GetComponent<ObjectIdentifier>() == false) {
            go.AddComponent<ObjectIdentifier>();
        }

        ObjectIdentifier idScript = go.GetComponent<ObjectIdentifier>();
        idScript.id = sceneObject.id;
        idScript.idParent = sceneObject.idParent;

        UnpackComponents(ref go, sceneObject);

        //Destroy any children that were not referenced as having a parent
        ObjectIdentifier[] childrenIds = go.GetComponentsInChildren<ObjectIdentifier>();
        foreach (ObjectIdentifier childrenIDScript in childrenIds) {
            if (childrenIDScript.transform != go.transform) {
                if (string.IsNullOrEmpty(childrenIDScript.id) == true) {
                    Destroy(childrenIDScript.gameObject);
                }
            }
        }
        return go;
    }

    public void UnpackSaveGame(SaveGame game, bool clearScene=true) {
        if (clearScene) { ClearScene(); }

        List<GameObject> goList = new List<GameObject>();

        //iterate through the loaded game's sceneObjects list to access each stored objet's data and reconstruct it with all it's components
        foreach (SceneObject loadedObject in game.sceneObjects) {
            GameObject newGo;
            if ((newGo = UnpackGameObject(loadedObject)) != null) {
                //Add the reconstructed GO to the list we created earlier.
                goList.Add(newGo);
            }
        }

        //Go through the list of reconstructed GOs and reassign any missing children
        foreach (GameObject go in goList) {
            var identifier = go.GetComponent<ObjectIdentifier>() as ObjectIdentifier;
            string parentId = identifier.idParent;
            saveLoadContext.oidReferences.Add(identifier.id, identifier);

            if (string.IsNullOrEmpty(parentId) == false) {
                foreach (GameObject go_parent in goList) {
                    if (go_parent.GetComponent<ObjectIdentifier>().id == parentId) {
                        //localscale is already where we want it after setting parent, so store it and reset it after setting parent
                        Vector3 storedScale = go.transform.localScale;
                        go.transform.parent = go_parent.transform;
                        go.transform.localScale = storedScale;
                    }
                }
            }
        }

        //resolve GameObject/MonoBehaviour references
        foreach (IReferenceResolver r in saveLoadContext.refResolvers) {
            r.Resolve();
        }

        //This is when you might want to call any functions that should be called when a gameobject is loaded. Example below.
        foreach (GameObject go2 in goList) {
            go2.SendMessage("OnDeserialize", SendMessageOptions.DontRequireReceiver);
        }
    }

    public SaveGame DeserializeSaveGame(IFormatter formatter, Stream stream) {
        return (SaveGame)formatter.Deserialize(stream);
    }

    public void SerializeSaveGame(IFormatter formatter, Stream stream, SaveGame sgame) {
        formatter.Serialize(stream, sgame);
    }

    public void SaveGame(string name="Quick") {
        try {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            Debug.Log(sw.ElapsedMilliseconds);
            StreamingContext ctx = new StreamingContext(StreamingContextStates.All, saveLoadContext);
            SurrogateSelector ss = getSurrogateSelector(ctx);
            BinaryFormatter bf = new BinaryFormatter(ss, ctx);
            var path = Application.persistentDataPath + "/Saved Games/" + name + ".sav";
            SaveGame game = PackSaveGame(name);
            using (Stream stream = File.Create(path)) {
                SerializeSaveGame(bf, stream, game);
            }
        } catch (Exception e) {
            Debug.Log(e);
        }
    }

    public void LoadGame(string name="Quick") {
        try {
            saveLoadContext = new SaveLoadContext();
            StreamingContext ctx = new StreamingContext(StreamingContextStates.All, saveLoadContext);
            SurrogateSelector ss = getSurrogateSelector(ctx);
            BinaryFormatter bf = new BinaryFormatter(ss, ctx);
            var path = Application.persistentDataPath + "/Saved Games/" + name + ".sav";
            using (Stream stream = File.Open(path, FileMode.Open)) {
                SaveGame game = DeserializeSaveGame(bf, stream);
                var sw = System.Diagnostics.Stopwatch.StartNew();
                UnpackSaveGame(game);
                Debug.Log(sw.ElapsedMilliseconds);
            }
        } catch (Exception e) {
            Debug.Log(e);
        }
    }
}
