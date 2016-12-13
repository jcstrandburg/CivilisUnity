using UnityEngine;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;


#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(SaverLoader))]
public class SaverLoaderEditor : Editor {

    private SaverLoader sl {
        get { return (SaverLoader)target; }
    }

    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        if (GUILayout.Button("QuickSave")) {
            sl.SaveGame();
        }
        if (GUILayout.Button("QuickLoad")) {
            if (!Application.isPlaying) {
                throw new InvalidOperationException("Must be used in play mode!");
            }
            sl.LoadGame();
        }
    }
}

#endif


public class SaverLoader : MonoBehaviour {
    private SaveLoadContext saveLoadContext = new SaveLoadContext();
    private Dictionary<string, GameObject> prefabDictionary;
    private List<string> surrogateSerializeFields = null;
    private List<string> customSerializeFields = null;
    public string loadIntoScene;


    /// <summary>
    /// Caches and returns a list of all prefabs existing in the project
    /// </summary>
    /// <returns>The cached prefabs</returns>
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

    /// <summary>
    /// Assembles a new SurrogateSelector with the given StreamingContext and 
    /// with all surrogates to be used. Also caches a list of all classes with surrogates
    /// in the context for use in determining which fields to serialize.
    /// </summary>
    /// <param name="context">A StreamingContext object containing the current SaveLoadContext</param>
    /// <returns>The SurrogateSelector with all surrogates added</returns>
    /// <todo>Refactor this to use LINQ (after save/load works again)</todo>
    private SurrogateSelector GetSurrogateSelector(StreamingContext context) {
        //cache a list of fields requiring surrogates
        if (surrogateSerializeFields == null) {
            surrogateSerializeFields = new List<string>() {
                "Vector3", "Texture2D", "Color", "GameObject", "Transform", "Quaternion"
            };

            //find all classes having the CustomSerialize attribute, we need a surrogate for these
            customSerializeFields = new List<string>();
            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes()) {
                //check if this type or any types it is derived from have the CustomSerialize attribute
                if (type.GetCustomAttributes(typeof(CustomSerialize), true).Length > 0) {
                    customSerializeFields.Add(type.Name);
                }
            }
        }

        //add the list of classes with surrogates to the SaveLoadContext
        saveLoadContext.autoSerializeTypes = surrogateSerializeFields.Concat(customSerializeFields).ToList();

        //add predetermined list of surrogates
        var s = new SurrogateSelector();
        s.AddSurrogate(typeof(Vector3), context, new Vector3Surrogate());
        s.AddSurrogate(typeof(Texture2D), context, new Texture2DSurrogate());
        s.AddSurrogate(typeof(Color), context, new ColorSurrogate());
        s.AddSurrogate(typeof(GameObject), context, new GameObjectSurrogate());
        s.AddSurrogate(typeof(Transform), context, new TransformSurrogate());
        s.AddSurrogate(typeof(Quaternion), context, new QuaternionSurrogate());

        //add a surrogate for all CustomSerialize fields
        foreach (string typeName in customSerializeFields) {
            var surrogate = new CustomSerializedObjectSurrogate();
            s.AddSurrogate(Type.GetType(typeName), context, surrogate);
        }
        return s;
    }

    /// <summary>
    /// Packs the fields of a single unity Component into an ObjectCompenent
    /// </summary>
    /// <param name="component">The component to be packed</param>
    /// <returns></returns>
    public ObjectComponent PackComponent(object component) {
        ObjectComponent newObjectComponent = new ObjectComponent();
        const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
        Type componentType = component.GetType();
        FieldInfo[] fields = componentType.GetFields(flags);

        newObjectComponent.componentName = componentType.ToString();
        foreach (FieldInfo field in fields) {
            newObjectComponent.fields.Add(field.Name + "DontSave?", field.GetCustomAttributes(typeof(DontSaveField), true).Length);
            if (field.GetCustomAttributes(typeof(DontSaveField), true).Length > 0) {
                continue;
            }                

            if (field.FieldType == typeof(GameObject)) {
                //create a resolver for GameObjects since they are not serialized normally
                GameObject g;
                ObjectIdentifier oid;

                //make sure the object has a unique id (only happens to objects being saved)
                if (   (g = field.GetValue(component) as GameObject) != null
                    && (oid = g.GetComponent<ObjectIdentifier>()) != null
                    && oid.HasID) {
                    newObjectComponent.references.Add(field.Name, new UnityObjectReference(oid.id, field.FieldType.Name, g.name));
                }
                continue;
            }
            else if (field.FieldType.IsSubclassOf(typeof(MonoBehaviour))) {
                MonoBehaviour mb;
                ObjectIdentifier oid;
                if ((mb = field.GetValue(component) as MonoBehaviour) != null
                    && (oid = mb.GetComponent<ObjectIdentifier>()) != null
                    && oid.HasID) {
                    newObjectComponent.references.Add(field.Name, new UnityObjectReference(oid.id, field.FieldType.Name, mb.name));
                }
                continue;
            }
            else if (saveLoadContext.FieldSerializeable(field)) {
                object value = field.GetValue(component);
                newObjectComponent.fields.Add(field.Name, value);
            } else {
                //can't serialize this field
            }               
        }
        return newObjectComponent;
    }

    /// <summary>
    /// Packs all serializeable components and important fields for a  single GameObject
    /// into a SceneObject
    /// </summary>
    /// <param name="go">The GameObject to be packed</param>
    /// <returns></returns>
    public SceneObject PackGameObject(GameObject go) {
        ObjectIdentifier objectIdentifier = go.GetComponent<ObjectIdentifier>();
        SceneObject sceneObject = new SceneObject();
        ObjectIdentifier parentID;

        //Assign all the GameObject's ids and misc. values
        sceneObject.name = go.name;
        sceneObject.prefabName = objectIdentifier.prefabName;
        sceneObject.id = objectIdentifier.id;
        sceneObject.position = go.transform.position;
        sceneObject.localScale = go.transform.localScale;
        sceneObject.rotation = go.transform.rotation;
        sceneObject.active = go.activeSelf;

        //look for a unique id on the parent and save it
        if (   go.transform.parent != null 
            && (parentID = go.transform.parent.GetComponent<ObjectIdentifier>()) != null) 
        {
            sceneObject.idParent = parentID.id;
        }
        else {
            sceneObject.idParent = null;
        }

        //pack each component
        List<Type> componentTypesToAdd = new List<Type>() { typeof(MonoBehaviour) };
        object[] components = go.GetComponents<Component>() as object[];
        foreach (object component_raw in components) {
            foreach (Type t in componentTypesToAdd) {
                if (component_raw.GetType().IsSubclassOf(t)) {
                    ObjectComponent comp = PackComponent(component_raw);
                    sceneObject.objectComponents.Add(comp);
                }
            }
        }

        return sceneObject;
    }

    /// <summary>
    /// Searches through the current scene for saveable objects and packs them all
    /// into a SaveGame obejct
    /// </summary>
    /// <param name="name">The name to assign to the SaveGame object</param>
    /// <returns></returns>
    public SaveGame PackSaveGame(string name) {
        SaveGame newSaveGame = new SaveGame();
        newSaveGame.savegameName = name;

        //go through all GameObjects with an ObjectIdentifier component, make sure they
        //all have ids set, add 'em to our list of stuff to save
        List<GameObject> goList = new List<GameObject>();
        ObjectIdentifier[] objectsToSerialize = FindObjectsOfType(typeof(ObjectIdentifier)) as ObjectIdentifier[];
        foreach (var objectIdentifier in objectsToSerialize) {
            if (objectIdentifier.dontSave == true) {
                continue;
            }

            if (string.IsNullOrEmpty(objectIdentifier.name)) {
                Debug.Log("No identifier on GameObject " + objectIdentifier.gameObject.name);
            }
            objectIdentifier.SetID();
            goList.Add(objectIdentifier.gameObject);
        }

        //send a message to all objects before going any further, make sure they
        //are prepared for serialization
        foreach (GameObject go in goList) {
            go.SendMessage("OnSerialize", SendMessageOptions.DontRequireReceiver);
        }

        foreach (GameObject go2 in goList) {
            newSaveGame.sceneObjects.Add(PackGameObject(go2));
        }
        return newSaveGame;
    }

    /// <summary>
    /// Removes all GameObjects not having the DontDestroy flag set, used to reset the scene before loading a save
    /// </summary>
    private void ClearScene() {
        object[] obj = GameObject.FindObjectsOfType(typeof(GameObject));
        foreach (object o in obj) {
            GameObject go = (GameObject)o;
            if (!go.CompareTag("DontDestroy")) {
                Destroy(go);
            }
        }
    }

    /// <summary>
    /// Unpacks all ObjectComponents from a SceneObject and attaches the resulting Components to the give GameObject
    /// </summary>
    /// <param name="go">GameObect to attach the Components to</param>
    /// <param name="sceneObject">source of the components</param>
    private void UnpackComponents(ref GameObject go, SceneObject sceneObject) {
        foreach (ObjectComponent obc in sceneObject.objectComponents) {
            if (go.GetComponent(obc.componentName) == false) {
                Type componentType = Type.GetType(obc.componentName);
                go.AddComponent(componentType);
            }

            //set the values for all basic deserializeable fields
            object component = go.GetComponent(obc.componentName) as object;
            var type = component.GetType();
            foreach (KeyValuePair<string, object> p in obc.fields) {
                var fld = type.GetField(p.Key,
                                      BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic |
                                      BindingFlags.SetField);
                if (fld != null) {
                    object value = p.Value;
                    // This may not work as intended, for example if this object is a collection or enumeration of serialiazables
                    //MethodInfo method = fld.FieldType.GetMethod("OnDeserialize", new Type[] { });
                    //if (method != null) {
                    //    method.Invoke(value, new object[] { });
                    //}
                    fld.SetValue(component, value);
                }
            }

            //create a reference resolver to be called upon once all objects have been deserialized
            foreach (string fieldName in obc.references.Keys) {
                UnityObjectReference r = obc.references[fieldName];
                FieldInfo f = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (f == null) {
                    Debug.Log("Unable to resolve fieldinfo for " + fieldName + " on "+obc.componentName);
                } else {
                    var resolver = new UnityObjectReferenceResolver(f, component, r, saveLoadContext);
                    saveLoadContext.refResolvers.Add(resolver);
                }
            }
        }
    }

    /// <summary>
    /// Unpacks and deserializes and entire Gameobject from a SceneObject
    /// </summary>
    /// <param name="sceneObject"></param>
    /// <returns></returns>
    private GameObject UnpackGameObject(SceneObject sceneObject) {
        //recreate the base object from the prefab
        Dictionary<string, GameObject> prefabs = getPrefabs();
        if (prefabs.ContainsKey(sceneObject.prefabName) == false) {
            Debug.Log("Can't find key " + sceneObject.prefabName + " in SaveLoadMenu.prefabDictionary!");
            return null;
        }

        //instantiate and reassign misc values
        GameObject go = Instantiate(prefabs[sceneObject.prefabName], sceneObject.position, sceneObject.rotation) as GameObject;
        sceneObject.objectInstance = go;
        go.name = sceneObject.name;
        go.transform.localScale = sceneObject.localScale;
        go.SetActive(sceneObject.active);

        ObjectIdentifier idScript = go.GetComponent<ObjectIdentifier>();
        idScript.id = sceneObject.id;
        idScript.idParent = sceneObject.idParent;

        UnpackComponents(ref go, sceneObject);

        //Destroy any children with ObjectIdentifiers don't currently have an id
        //this might happen if the prefab has children with ObjectIdentifiers on them but
        //for whatever reason the children didn't get saved. In that case they need to 
        //go awawy.
        ObjectIdentifier[] childrenIds = go.GetComponentsInChildren<ObjectIdentifier>();
        foreach (ObjectIdentifier childrenIDScript in childrenIds) {
            if (   childrenIDScript.transform != go.transform 
                && childrenIDScript.HasID == false) 
            {
                Destroy(childrenIDScript.gameObject);
            }
        }
        return go;
    }

    /// <summary>
    /// Unpacks the contents of a SaveGame object into the current scene
    /// </summary>
    /// <param name="game"></param>
    /// <param name="clearScene">Whether to remove all object currently in the scene (except those tagged as DontDestroy)</param>
    public void UnpackSaveGame(SaveGame game, bool clearScene=true) {
        if (clearScene) { ClearScene(); }
        List<GameObject> goList = new List<GameObject>();

        //iterate through the loaded game's sceneObjects list to access each stored objet's data and reconstruct it with all it's components
        foreach (SceneObject loadedObject in game.sceneObjects) {
            GameObject newGo;
            if ((newGo = UnpackGameObject(loadedObject)) != null) {
                goList.Add(newGo);
            }
        }

        //build a dictionary referencing unique ids to ObjectIdentifier instances for faster lookup
        foreach (GameObject go in goList) {
            var identifier = go.GetComponent<ObjectIdentifier>() as ObjectIdentifier;
            saveLoadContext.oidReferences.Add(identifier.id, identifier);
        }

        //Go through the list of reconstructed GOs and reattach them to their parents,
        //Also store
        foreach (GameObject go in goList) {
            var identifier = go.GetComponent<ObjectIdentifier>() as ObjectIdentifier;
            string parentId = identifier.idParent;
            if (string.IsNullOrEmpty(parentId) == false) {
                ObjectIdentifier oid = saveLoadContext.oidReferences[parentId];
                //localscale is already where we want it after setting parent, so store it and reset it after setting parent
                Vector3 storedScale = go.transform.localScale;
                go.transform.parent = oid.transform;
                go.transform.localScale = storedScale;
            }
        }

        //now that everything is reconstructed, resolve references and call any OnDeserialize methods
        foreach (IReferenceResolver r in saveLoadContext.refResolvers) {
            r.Resolve();
        }
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
            StreamingContext ctx = new StreamingContext(StreamingContextStates.All, saveLoadContext);
            SurrogateSelector ss = GetSurrogateSelector(ctx);
            IFormatter bf = new BinaryFormatter(ss, ctx);
            //bf.AssemblyFormat = FormatterAssemblyStyle.Simple; //this isn't doing what I think it should
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
        if (string.IsNullOrEmpty(loadIntoScene)) {
            try {
                saveLoadContext = new SaveLoadContext();
                StreamingContext ctx = new StreamingContext(StreamingContextStates.All, saveLoadContext);
                SurrogateSelector ss = GetSurrogateSelector(ctx);
                IFormatter bf = new BinaryFormatter(ss, ctx);
                //bf.AssemblyFormat = FormatterAssemblyStyle.Simple; //this isn't doing what I think it should
                var path = Application.persistentDataPath + "/Saved Games/" + name + ".sav";
                using (Stream stream = File.Open(path, FileMode.Open)) {
                    SaveGame game = DeserializeSaveGame(bf, stream);
                    var sw = System.Diagnostics.Stopwatch.StartNew();
                    UnpackSaveGame(game);
                    Debug.Log(sw.ElapsedMilliseconds);
                }
            }
            catch (Exception e) {
                Debug.Log(e);
            }
        } else {
            GameObject go = new GameObject();
            var transition = go.AddComponent<GameSceneTransitioner>();
            transition.InitLoadGame(name);
            SceneManager.LoadScene(loadIntoScene);
        }
    }

    public string[] GetSaveGames() {
        string path = Application.persistentDataPath + "/Saved Games/";
        return Directory
            .GetFiles(path)
            .Select<string, string>(Path.GetFileNameWithoutExtension)
            .ToArray();
    }
}
