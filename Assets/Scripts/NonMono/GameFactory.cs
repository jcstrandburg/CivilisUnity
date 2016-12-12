using UnityEngine;
using System.Collections;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using System;

/// <summary>
/// Helper factory class that handles dependency injection/service location
/// </summary>
public class GameFactory {

    [Injectable]
    public GameFactory Factory {
        get { return this; }
    }

    private GameController gameController = null;
    [Injectable]
    public GameController GameController {
        get {
            if (gameController == null) {
                gameController = GameObject.FindObjectOfType<GameController>();
            }
            return gameController;
        }
    }

    private GameUIController guiController;
    [Injectable]
    public GameUIController GuiController {
        get {
            if (guiController == null) {
                guiController = GameObject.FindObjectOfType<GameUIController>();
            }
            return guiController;
        }
        set {
            guiController = value;
        }
    }

    private GroundController groundController;
    [Injectable]
    public GroundController GroundController {
        get {
            if (groundController == null) {
                groundController = GameObject.FindObjectOfType<GroundController>();
            }
            return groundController;
        }
        set {
            groundController = value;
        }
    }

    private StatManager statManager;
    [Injectable]
    public StatManager StatManager {
        get {
            if (statManager == null) {
                statManager = GameObject.FindObjectOfType<StatManager>();
            }
            return statManager;
        }
        set {
            statManager = value;
        }
    }

    private SaverLoader saverLoader;
    [Injectable]
    public SaverLoader SaverLoader {
        get {
            if (saverLoader == null) {
                saverLoader = GameObject.FindObjectOfType<SaverLoader>();
            }
            return saverLoader;
        }
        set {
            saverLoader = value;
        }
    }

    private MenuManager menuManager;
    [Injectable]
    public MenuManager MenuManager {
        get {
            if (menuManager == null) {
                menuManager = GameObject.FindObjectOfType<MenuManager>();
            }
            return menuManager;
        }
        set {
            menuManager = value;
        }
    }

    private LogisticsManager logisticsManager;
    [Injectable]
    public LogisticsManager LogisticsManager {
        get {
            if (logisticsManager == null) {
                logisticsManager = GameObject.FindObjectOfType<LogisticsManager>();
            }
            return logisticsManager;
        }
        set {
            logisticsManager = value;
        }
    }

    private DayCycleController dayCycleController;
    [Injectable]
    public DayCycleController DayCycleController {
        get {
            if (dayCycleController == null) {
                dayCycleController = GameObject.FindObjectOfType<DayCycleController>();
            }
            return dayCycleController;
        }
        set {
            dayCycleController = value;
        }
    }

    /// <summary>Injectable fields from this class</summary>
    private Dictionary<Type, FieldInfo> myFields;
    /// <summary>Injectable properties from this class</summary>
    private Dictionary<Type, PropertyInfo> myProps;
    /// <summary>Injectable fields by object type</summary>
    private readonly Dictionary<Type, FieldInfo[]> fieldInfoCache = new Dictionary<Type, FieldInfo[]>();
    /// <summary>Injectable properties by object type</summary>
    private readonly Dictionary<Type, PropertyInfo[]> propInfoCache = new Dictionary<Type, PropertyInfo[]>();

    public GameFactory() {
        myFields = GetType()
            .GetFields()
            .Where(field => field.IsDefined(typeof(Injectable), false))
            .ToDictionary(
                ele => ele.FieldType,
                ele => ele);
        myProps = GetType()
            .GetProperties()
            .Where(prop => prop.IsDefined(typeof(Injectable), false))
            .ToDictionary(
                ele => ele.PropertyType,
                ele => ele);
    }

    /// <summary>
    /// Instatiates and injects a copy of the given GameObject (assumed to be a prefab)
    /// </summary>
    /// <param name="prefab"></param>
    /// <returns>The instantiated object</returns>
    public GameObject Instantiate(GameObject prefab) {
        var instance = GameObject.Instantiate(prefab);
        InjectGameobject(instance);
        return instance;
    }

    /// <summary>
    /// Gets all Fields for the given type that can be injected.
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    private FieldInfo[] GetInjectableFields(Type t) {
        const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;

        if (!fieldInfoCache.ContainsKey(t)) {
            fieldInfoCache[t] = t
                .GetFields(flags)
                .Where(field => field.IsDefined(typeof(Inject), false))
                .ToArray();
        }
        return fieldInfoCache[t];
    }

    /// <summary>
    /// Gets all Properties for the given type that can be injected. The results will be cached.
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    private PropertyInfo[] GetInjectableProps(Type t) {
        const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;

        if (!propInfoCache.ContainsKey(t)) {
            propInfoCache[t] = t
                .GetProperties(flags)
                .Where(prop => prop.IsDefined(typeof(Inject), false))
                .ToArray();
        }
        return propInfoCache[t];
    }

    /// <summary>
    /// Injects an individual object from the Injectable fields and properties of this object
    /// </summary>
    /// <param name="injectme"></param>
    /// <returns>The object passed in</returns>
    public T InjectObject<T>(T injectme) {
        if (injectme == null) {
            throw new ArgumentException("Given null object to inject");
        }
        var compType = injectme.GetType();

        //get fields and properties to be injected
        var compFields = GetInjectableFields(compType);
        var compProperties = GetInjectableProps(compType);

        foreach (var compField in compFields) {
            var type = compField.FieldType;

            if (myFields.ContainsKey(type)) {
                var sourceField = myFields[type];
                compField.SetValue(injectme, sourceField.GetValue(this));
            }

            if (myProps.ContainsKey(type)) {
                var sourceProp = myProps[type];
                compField.SetValue(injectme, sourceProp.GetValue(this, null));
            }
        }

        foreach (var compProp in compProperties) {
            var type = compProp.PropertyType;

            if (myFields.ContainsKey(type)) {
                var sourceField = myFields[type];
                compProp.SetValue(injectme, sourceField.GetValue(this), null);
            }

            if (myProps.ContainsKey(type)) {
                var sourceProp = myProps[type];
                compProp.SetValue(injectme, sourceProp.GetValue(this, null), null);
            }
        }

        return injectme;
    }

    /// <summary>
    /// Injects all fields marked with the "inject" attribute in all components in the
    /// given object and all of its children. The injected value is taken from fields
    /// or properties on this object with the "injectable" attribute
    /// </summary>
    /// <param name="obj">The object to be injected</param>
    public void InjectGameobject(GameObject obj) {
        var components = obj.GetComponentsInChildren<Component>();
        foreach (var component in components) {
            InjectObject(component);
        }
    }

    /// <summary>
    /// Adds the given component type to the given game object and then injects it with dependencies
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="go"></param>
    /// <returns>The component added</returns>
    public T AddComponent<T>(GameObject go) where T : MonoBehaviour {
        var t = go.AddComponent<T>();
        InjectObject(t);
        return t;
    }
}
